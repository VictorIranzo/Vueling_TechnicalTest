namespace Vueling.OTD.Logic
{
    using Microsoft.EntityFrameworkCore;
    using Polly;
    using QuikGraph;
    using QuikGraph.Algorithms;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Vueling.OTD.Contracts;
    using Vueling.OTD.Contracts.DTOs;
    using Vueling.OTD.Contracts.Enumerations;
    using Vueling.OTD.Logic.Clients;
    using Vueling.OTD.Logic.Converters;
    using Vueling.OTD.Persistence.Entities;

    internal class TransactionsManager : ITransactionsManager
    {
        private readonly Persistence.Context context;
        private readonly IRatesManager ratesManager;
        private readonly IConverter<APITransaction, Transaction> apiTransactionsToTransactionConverter;
        private readonly IConverter<Transaction, TransactionDTO> transactionsToTransactionDtoConverter;
        private readonly PollyPolicies pollyPolicies;
        private readonly IClientFactory clientFactory;

        public TransactionsManager(
            Persistence.Context context,
            IRatesManager ratesManager,
            IConverter<APITransaction, Transaction> apiTransactionsToTransactionConverter,
            IConverter<Transaction, TransactionDTO> transactionsToTransactionDtoConverter,
            PollyPolicies pollyPolicies,
            IClientFactory clientFactory)
        {
            this.context = context;
            this.ratesManager = ratesManager;
            this.apiTransactionsToTransactionConverter = apiTransactionsToTransactionConverter;
            this.transactionsToTransactionDtoConverter = transactionsToTransactionDtoConverter;
            this.pollyPolicies = pollyPolicies;
            this.clientFactory = clientFactory;
        }

        public async Task<IEnumerable<TransactionDTO>> GetAllTransactionsAsync()
        {
            APIClient apiClient = this.clientFactory.GetClient();

            PolicyResult<ICollection<APITransaction>> pollyResultFromClient =
                await this.pollyPolicies.RetryPolicy.ExecuteAndCaptureAsync(apiClient.GetTransactionsAsync).ConfigureAwait(false);

            if (pollyResultFromClient.Outcome == OutcomeType.Successful)
            {
                // Updates the existing values at the database and returns them.
                IEnumerable<Transaction> newTransactions = this.apiTransactionsToTransactionConverter.Convert(pollyResultFromClient.Result);

                this.UpdateTransactionsAtDatabase(newTransactions);

                return this.transactionsToTransactionDtoConverter.Convert(newTransactions);
            }
            else
            {
                // If an error occurs invoking the client after the Polly retries,
                // returns the values stored at the database.
                return this.transactionsToTransactionDtoConverter.Convert(this.context.Transactions);
            }
        }

        public async Task<SkuTransactionsDTO> GetTransactionsBySkuInEurosAsync(string sku)
        {
            IEnumerable<Transaction> transactions = this.context.Transactions
                .AsNoTracking()
                .Where(t => t.SKU == sku).AsEnumerable();

            IEnumerable<Transaction> transactionsInEuros = await this.ConvertTransactionsToCurrencyAsync(transactions, outputCurrency: Currency.Euro).ConfigureAwait(false);

            return new SkuTransactionsDTO()
            {
                Transactions = this.transactionsToTransactionDtoConverter.Convert(transactionsInEuros).OrderByDescending(t => t.Amount).ToList(),
                SumTransactionsInEuros = Math.Round(transactionsInEuros.Sum(t => t.Amount), digits: 2, MidpointRounding.ToEven),
            };
        }

        private async Task<IEnumerable<Transaction>> ConvertTransactionsToCurrencyAsync(IEnumerable<Transaction> transactions, Currency outputCurrency)
        {
            (AdjacencyGraph<Currency, Edge<Currency>> graph, Dictionary<Edge<Currency>, double> costDictionary) = await this.GetRatesGraphAsync().ConfigureAwait(false);
            List<Transaction> result = new List<Transaction>();

            foreach (Transaction transaction in transactions)
            {
                Currency inputCurrency = transaction.Currency;

                if (inputCurrency == outputCurrency)
                {
                    result.Add(transaction);

                    continue;
                }

                if (!graph.TryGetEdge(inputCurrency, outputCurrency, out Edge<Currency> edge) || !costDictionary.TryGetValue(edge, out double conversion))
                {
                    throw new InvalidOperationException($"A conversion from '{inputCurrency}' to '{outputCurrency}' has not been found.");
                }

                transaction.Currency = outputCurrency;
                transaction.Amount = Math.Round(transaction.Amount * conversion, digits: 2, MidpointRounding.ToEven);

                result.Add(transaction);
            }

            return result;
        }

        // TODO: Study if this object is heavy to be computed and store it in a cache if so.

        internal async Task<(AdjacencyGraph<Currency, Edge<Currency>> graph, Dictionary<Edge<Currency>, double> costDictionary)> GetRatesGraphAsync()
        {
            AdjacencyGraph<Currency, Edge<Currency>> graph = new AdjacencyGraph<Currency, Edge<Currency>>();
            Dictionary<Edge<Currency>, double> costDictionary = new Dictionary<Edge<Currency>, double>();

            IEnumerable<RateDTO> existingRates = await this.ratesManager.GetAllRatesAsync().ConfigureAwait(false);

            foreach (RateDTO rate in existingRates)
            {
                AddEdge(rate.FromCurrency, rate.ToCurrency, rate.Rate);

                // Adds also the inverse edge, that can be computed as 1 / ratio. Note that the
                // rounding is using 3 digits because it follows the example rates.
                AddEdge(rate.ToCurrency, rate.FromCurrency, Math.Round((1 / rate.Rate), digits: 3, MidpointRounding.ToEven));
            }

            // Adds the missing conversions. The result must be a complete
            // graph, that is a graph where every vertex is related with each
            // other.
            int numberOfEdgesPerVertexToBeCompleted = graph.Vertices.Count() - 1;

            foreach (Currency vertex in graph.Vertices)
            {
                if (!graph.TryGetOutEdges(vertex, out IEnumerable<Edge<Currency>> vertexOutEdges))
                {
                    throw new KeyNotFoundException($"The currency '{vertex}' was not found at the graph to represent rates.");
                }

                // The vertex contains all required edges, so no inference is required.
                if (vertexOutEdges.Count() == numberOfEdgesPerVertexToBeCompleted)
                {
                    continue;
                }

                TryFunc<Currency, IEnumerable<Edge<Currency>>> shortestPathsDijkstraResult =
                    graph.ShortestPathsDijkstra((edge) => costDictionary[edge], vertex);

                IEnumerable<Currency> existingRelatedVertices = vertexOutEdges.Select(e => e.Target);
                IEnumerable<Currency> verticesMissingEdge = graph.Vertices.Except(
                    existingRelatedVertices.Union(new List<Currency>() { vertex }));

                foreach (Currency vertexMissingEdge in verticesMissingEdge)
                {
                    if (!shortestPathsDijkstraResult(vertexMissingEdge, out IEnumerable<Edge<Currency>> shortestPath))
                    {
                        throw new InvalidOperationException($"A path from the currency '{vertex}' to the currency '{vertexMissingEdge}' has not been found. The conversion between them can not be done.");
                    }

                    // The new Edge cost is computed traversing the path and multiplying
                    // the cost of every path.
                    double newEdgeCost = Math.Round(
                        shortestPath.Aggregate((double)1, (acc, e) => acc * costDictionary[e]),
                        digits: 3,
                        MidpointRounding.ToEven);

                    AddEdge(vertex, vertexMissingEdge, newEdgeCost);
                }
            }

            return (graph, costDictionary);

            void AddEdge(Currency sourceCurrency, Currency targetCurrency, double cost)
            {
                Edge<Currency> edge = new Edge<Currency>(
                    source: sourceCurrency,
                    target: targetCurrency);

                graph.AddVerticesAndEdge(edge);
                costDictionary.Add(edge, cost);
            }
        }

        private void UpdateTransactionsAtDatabase(IEnumerable<Transaction> newTransactions)
        {
            this.context.Transactions.RemoveAllRows();
            this.context.Transactions.AddRange(newTransactions);

            this.context.SaveChanges();
        }
    }
}
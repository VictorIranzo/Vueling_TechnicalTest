namespace Vueling.OTD.Logic.Tests.UnitTests
{
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using QuikGraph;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Vueling.OTD.Contracts;
    using Vueling.OTD.Contracts.DTOs;
    using Vueling.OTD.Contracts.Enumerations;
    using Vueling.OTD.Logic.Tests.Mocks;
    using Vueling.OTD.Persistence;
    using Vueling.OTD.Persistence.Entities;

    [TestClass]
    public class TransactionsTests
    {
        /// <summary>
        ///     The method <see
        ///     cref="ITransactionsManager.GetTransactionsBySkuInEurosAsync(string)"/>, when it is
        ///     invoked for a SKU in which the currency of one transaction to Euros is missing,
        ///     returns the expected transactions in Euros.
        /// </summary>
        /// <returns> A task that enables this method to be awaited. </returns>
        [TestMethod]
        public async Task GetTransactionsBySkuInEurosAsync_MissingConversions_ReturnsTransactionsInEuros()
        {
            // Arrange.
            void AdditionalRegistrations(IServiceCollection services)
            {
                RatesManagerMock ratesManager = new RatesManagerMock().MockGetAllRates(outputRates: new List<RateDTO>()
                { 
                    new RateDTO()
                    {
                        FromCurrency = Currency.Euro,
                        ToCurrency = Currency.AustralianDollar,
                        Rate = 1.36,
                    },
                    new RateDTO()
                    {
                        FromCurrency = Currency.UnitedStatesDollar,
                        ToCurrency = Currency.Euro,
                        Rate = 0.73,
                    },
                });

                services.Remove(services.First(s => s.ServiceType == typeof(IRatesManager)));

                services.AddSingleton<IRatesManager>((s) => ratesManager.Object);
            }

            IServiceProvider serviceProvider = TestsServiceProvider.GetServiceProvider(AdditionalRegistrations);
            IServiceProvider arrangeServiceProvder = serviceProvider.CreateScope().ServiceProvider;

            using (Context context = arrangeServiceProvder.GetService<Context>())
            {
                context.Database.EnsureCreated();

                context.Transactions.Add(new Transaction()
                {
                    Id = Guid.NewGuid(),
                    SKU = "UT1",
                    Amount = 12.5,
                    Currency = Currency.Euro,
                });

                context.Transactions.Add(new Transaction()
                {
                    Id = Guid.NewGuid(),
                    SKU = "UT1",
                    Amount = 10,
                    Currency = Currency.AustralianDollar,
                });

                context.Transactions.Add(new Transaction()
                {
                    Id = Guid.NewGuid(),
                    SKU = "UT1",
                    Amount = 9,
                    Currency = Currency.UnitedStatesDollar,
                });

                context.Transactions.Add(new Transaction()
                {
                    Id = Guid.NewGuid(),
                    SKU = "UT2",
                    Amount = 100,
                    Currency = Currency.UnitedStatesDollar,
                });

                context.SaveChanges();
            }

            // Act.
            IServiceProvider actServiceProvder = serviceProvider.CreateScope().ServiceProvider;
            ITransactionsManager transactionsManager = actServiceProvder.GetService<ITransactionsManager>();

            SkuTransactionsDTO result = await transactionsManager.GetTransactionsBySkuInEurosAsync("UT1").ConfigureAwait(false);

            // Assert.
            SkuTransactionsDTO expectedResult = new SkuTransactionsDTO()
            {
                SumTransactionsInEuros = 26.42,
                Transactions = new List<TransactionDTO>()
                {
                    new TransactionDTO()
                    {
                        Amount = 12.5,
                        Currency = Currency.Euro,
                        SKU = "UT1",
                    },
                    new TransactionDTO()
                    {
                        Amount = 7.35,
                        Currency = Currency.Euro,
                        SKU = "UT1",
                    }, 
                    new TransactionDTO()
                    {
                        Amount = 6.57,
                        Currency = Currency.Euro,
                        SKU = "UT1",
                    },
                },
            };

            result.Should().BeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }

        /// <summary>
        ///     The method <see cref="TransactionsManager.GetRatesGraphAsync"/>, when not all rates
        ///     are returned, computes the missing edges to generate the graph that represents all conversions.
        /// </summary>
        /// <returns> A task that enables this method to be awaited. </returns>
        [TestMethod]
        public async Task GetRatesGraphAsync_MissingRates_ComputesAllEdges()
        {
            // Arrange.
            void AdditionalRegistrations(IServiceCollection services)
            {
                RatesManagerMock ratesManager = new RatesManagerMock().MockGetAllRates(outputRates: new List<RateDTO>()
                {
                    new RateDTO()
                    {
                        FromCurrency = Currency.Euro,
                        ToCurrency = Currency.AustralianDollar,
                        Rate = 1.36,
                    },
                    new RateDTO()
                    {
                        FromCurrency = Currency.UnitedStatesDollar,
                        ToCurrency = Currency.Euro,
                        Rate = 0.73,
                    },
                });

                services.Remove(services.First(s => s.ServiceType == typeof(IRatesManager)));

                services.AddSingleton<IRatesManager>((s) => ratesManager.Object);
            }

            IServiceProvider serviceProvider = TestsServiceProvider.GetServiceProvider(AdditionalRegistrations);
            IServiceProvider arrangeServiceProvder = serviceProvider.CreateScope().ServiceProvider;

            // Act.
            IServiceProvider actServiceProvder = serviceProvider.CreateScope().ServiceProvider;
            TransactionsManager transactionsManager = actServiceProvder.GetService<ITransactionsManager>() as TransactionsManager;

            (AdjacencyGraph<Currency, Edge<Currency>> graph, Dictionary<Edge<Currency>, double> costDictionary) = await transactionsManager.GetRatesGraphAsync().ConfigureAwait(false);

            // Assert.
            List<Currency> expectedVertices = new List<Currency>()
            {
                Currency.Euro,
                Currency.AustralianDollar,
                Currency.UnitedStatesDollar,
            };

            graph.Vertices.Should().BeEquivalentTo(expectedVertices);

            List<Edge<Currency>> expectedEdges = new List<Edge<Currency>>()
            {
                new Edge<Currency>(Currency.Euro, Currency.AustralianDollar),
                new Edge<Currency>(Currency.AustralianDollar, Currency.Euro),
                new Edge<Currency>(Currency.Euro, Currency.UnitedStatesDollar),
                new Edge<Currency>(Currency.UnitedStatesDollar, Currency.Euro),
                new Edge<Currency>(Currency.AustralianDollar, Currency.UnitedStatesDollar),
                new Edge<Currency>(Currency.UnitedStatesDollar, Currency.AustralianDollar),
            };

            graph.Edges.Should().BeEquivalentTo(expectedEdges);

            Dictionary<Edge<Currency>, double> expectedCostDictionary = new Dictionary<Edge<Currency>, double>()
            {
                { new Edge<Currency>(Currency.Euro, Currency.AustralianDollar), 1.36 },
                { new Edge<Currency>(Currency.AustralianDollar, Currency.Euro), 0.735 },
                { new Edge<Currency>(Currency.UnitedStatesDollar, Currency.Euro), 0.73 },
                { new Edge<Currency>(Currency.Euro, Currency.UnitedStatesDollar), 1.37 },
                { new Edge<Currency>(Currency.AustralianDollar, Currency.UnitedStatesDollar), 1.007 },
                { new Edge<Currency>(Currency.UnitedStatesDollar, Currency.AustralianDollar), 0.993 },
            };

            // ToList is done because Dictionary comparison does not work as expected.
            costDictionary.ToList().Should().BeEquivalentTo(expectedCostDictionary.ToList());
        }

        /// <summary>
        ///     The method <see cref="TransactionsManager.GetRatesGraphAsync"/>, when not all the
        ///     necessary rates are returned to compute the rates graph, an exception is thrown.
        /// </summary>
        /// <returns> A task that enables this method to be awaited. </returns>
        [TestMethod]
        public async Task GetRatesGraphAsync_MissingNecessaryConversions_ThrowsException()
        {
            // Arrange.
            void AdditionalRegistrations(IServiceCollection services)
            {
                RatesManagerMock ratesManager = new RatesManagerMock().MockGetAllRates(outputRates: new List<RateDTO>()
                {
                    new RateDTO()
                    {
                        FromCurrency = Currency.Euro,
                        ToCurrency = Currency.AustralianDollar,
                        Rate = 1.36,
                    },
                    new RateDTO()
                    {
                        FromCurrency = Currency.CanadianDollar,
                        ToCurrency = Currency.UnitedStatesDollar,
                        Rate = 0.73,
                    },
                });

                services.Remove(services.First(s => s.ServiceType == typeof(IRatesManager)));

                services.AddSingleton<IRatesManager>((s) => ratesManager.Object);
            }

            IServiceProvider serviceProvider = TestsServiceProvider.GetServiceProvider(AdditionalRegistrations);
            IServiceProvider arrangeServiceProvder = serviceProvider.CreateScope().ServiceProvider;

            // Act.
            IServiceProvider actServiceProvder = serviceProvider.CreateScope().ServiceProvider;
            TransactionsManager transactionsManager = actServiceProvder.GetService<ITransactionsManager>() as TransactionsManager;

            Func<Task> action = () => transactionsManager.GetRatesGraphAsync();

            // Assert.
            await action.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A path from the currency 'Euro' to the currency 'CanadianDollar' has not been found. The conversion between them can not be done.")
            .ConfigureAwait(false);
        }
    }
}
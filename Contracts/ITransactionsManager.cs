namespace Vueling.OTD.Contracts
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Vueling.OTD.Contracts.DTOs;

    public interface ITransactionsManager
    {
        Task<IEnumerable<TransactionDTO>> GetAllTransactionsAsync();

        Task<SkuTransactionsDTO> GetTransactionsBySkuInEurosAsync(string sku);
    }
}
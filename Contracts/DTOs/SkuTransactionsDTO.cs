namespace Vueling.OTD.Contracts.DTOs
{
    using System.Collections.Generic;

    public class SkuTransactionsDTO
    {
        public IEnumerable<TransactionDTO> Transactions { get; set; }

        public double SumTransactionsInEuros { get; set; }
    }
}
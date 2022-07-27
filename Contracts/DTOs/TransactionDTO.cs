namespace Vueling.OTD.Contracts.DTOs
{
    using Vueling.OTD.Contracts.Enumerations;

    public class TransactionDTO
    {
        public string SKU { get; set; }
        public double Amount { get; set; }
        public Currency Currency { get; set; }
    }
}
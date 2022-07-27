namespace Vueling.OTD.Persistence.Entities
{
    using Vueling.OTD.Contracts.Enumerations;

    public class Transaction : EntityBase
    {
        public string SKU { get; set; }
        public double Amount { get; set; }
        public Currency Currency { get; set; }
    }
}
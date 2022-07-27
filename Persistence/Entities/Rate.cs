namespace Vueling.OTD.Persistence.Entities
{
    using Vueling.OTD.Contracts.Enumerations;

    public class Rate : EntityBase
    {
        public Currency FromCurrency { get; set; }
        public Currency ToCurrency { get; set; }
        public double Ratio { get; set; }
    }
}
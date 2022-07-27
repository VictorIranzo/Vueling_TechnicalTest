namespace Vueling.OTD.Contracts.DTOs
{
    using Vueling.OTD.Contracts.Enumerations;

    public class RateDTO
    {
        public Currency FromCurrency { get; set; }
        public Currency ToCurrency { get; set; }
        public double Rate { get; set; }
    }
}
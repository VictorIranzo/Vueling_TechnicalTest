namespace Vueling.OTD.Logic.Converters
{
    using System.Diagnostics.CodeAnalysis;
    using Vueling.OTD.Contracts.DTOs;
    using Vueling.OTD.Persistence.Entities;

    internal class RateToRateDtoConverter : ConverterBase<Rate, RateDTO>
    {
        public override RateDTO Convert([NotNull] Rate input)
        {
            return new RateDTO()
            {
                Rate = input.Ratio,
                FromCurrency = input.FromCurrency,
                ToCurrency = input.ToCurrency,
            };
        }
    }
}
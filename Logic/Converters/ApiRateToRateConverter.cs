namespace Vueling.OTD.Logic.Converters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Vueling.OTD.Contracts.Enumerations;
    using Vueling.OTD.Logic.Clients;
    using Vueling.OTD.Persistence.Entities;

    internal class ApiRateToRateConverter : ConverterBase<APIRate, Rate>
    {
        private readonly IConverter<string, Currency> currencyConverter;

        public ApiRateToRateConverter(IConverter<string, Currency> currencyConverter)
        {
            this.currencyConverter = currencyConverter;
        }

        public override Rate Convert([NotNull] APIRate input)
        {
            if (!double.TryParse(input.Rate, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double ratio))
            {
                throw new ArgumentNullException(nameof(input), $"The ratio '{input.Rate}' can not be parsed to a double value.");
            }

            return new Rate()
            {
                Id = Guid.NewGuid(),
                FromCurrency = this.currencyConverter.Convert(input.From),
                ToCurrency = this.currencyConverter.Convert(input.To),
                Ratio = Math.Round(ratio, digits: 2, MidpointRounding.ToEven),
            };
        }
    }
}
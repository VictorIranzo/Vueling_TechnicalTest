namespace Vueling.OTD.Logic.Converters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Vueling.OTD.Contracts.Enumerations;
    using Vueling.OTD.Logic.Clients;
    using Vueling.OTD.Persistence.Entities;

    internal class ApiTransactionToTransactionConverter : ConverterBase<APITransaction, Transaction>
    {
        private readonly IConverter<string, Currency> currencyConverter;

        public ApiTransactionToTransactionConverter(IConverter<string, Currency> currencyConverter)
        {
            this.currencyConverter = currencyConverter;
        }

        public override Transaction Convert([NotNull] APITransaction input)
        {
            if (!double.TryParse(input.Amount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double amount))
            {
                throw new ArgumentNullException(nameof(input), $"The amount '{input.Amount}' can not be parsed to a double value.");
            }

            return new Transaction()
            {
                Id = Guid.NewGuid(),
                SKU = input.Sku,
                Currency = this.currencyConverter.Convert(input.Currency),
                Amount = Math.Round(amount, digits: 2, MidpointRounding.ToEven),
            };
        }
    }
}
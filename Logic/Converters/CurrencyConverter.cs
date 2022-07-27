namespace Vueling.OTD.Logic.Converters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Vueling.OTD.Contracts.Enumerations;

    internal class CurrencyConverter : ConverterBase<string, Currency>
    {
        public override Currency Convert([NotNull] string input)
        {
            switch (input)
            {
                case "EUR":
                    return Currency.Euro;
                case "USD":
                    return Currency.UnitedStatesDollar;
                case "CAD":
                    return Currency.CanadianDollar;
                case "AUD":
                    return Currency.AustralianDollar;
                default:
                    throw new ArgumentNullException(nameof(input), $"The currency '{input}' can not be converted.");
            }
        }
    }
}
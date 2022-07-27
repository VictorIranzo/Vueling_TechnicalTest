namespace Vueling.OTD.Logic.Converters
{
    using System.Diagnostics.CodeAnalysis;
    using Vueling.OTD.Contracts.DTOs;
    using Vueling.OTD.Persistence.Entities;

    internal class TransactionToTransactionDtoConverter
        : ConverterBase<Transaction, TransactionDTO>
    {
        public override TransactionDTO Convert([NotNull] Transaction input)
        {
            return new TransactionDTO()
            {
                Amount = input.Amount,
                Currency = input.Currency,
                SKU = input.SKU,
            };
        }
    }
}
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Vueling.OTD.Logic.Converters
{
    internal abstract class ConverterBase<TIn, TOut> : IConverter<TIn, TOut>
    {
        public IEnumerable<TOut> Convert([NotNull] IEnumerable<TIn> inputCollection)
        {
            foreach (TIn input in inputCollection)
            {
                yield return Convert(input);
            }
        }

        public abstract TOut Convert([NotNull] TIn input);
    }
}
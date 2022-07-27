namespace Vueling.OTD.Logic.Converters
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public interface IConverter<TIn, TOut>
    {
        IEnumerable<TOut> Convert([NotNull] IEnumerable<TIn> inputCollection);

        TOut Convert([NotNull] TIn input);
    }
}
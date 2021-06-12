using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// <see cref="AsteriskExpression"/> represent a <c>*</c> that can be used as a building block for more
    /// complex <see cref="FilterExpression"/>s.
    /// </summary>
#if NETSTANDARD1_3
    public sealed class AsteriskExpression : FilterExpression, IEquatable<AsteriskExpression>, IBoundaryExpression
    {
        ///<inheritdoc/>
        public bool Equals(AsteriskExpression other) => other is not null;
    }
#else
    public record AsteriskExpression : FilterExpression, IEquatable<AsteriskExpression>, IBoundaryExpression
    {
    }
#endif
}
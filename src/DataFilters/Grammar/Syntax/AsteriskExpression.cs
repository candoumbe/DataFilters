namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// <see cref="AsteriskExpression"/> represent a <c>*</c> that can be used as a building block for more
    /// complex <see cref="FilterExpression"/>s.
    /// </summary>
    public sealed class AsteriskExpression : FilterExpression, IEquatable<AsteriskExpression>, IBoundaryExpression
    {
        ///<inheritdoc/>
        public bool Equals(AsteriskExpression other) => other is not null;

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as AsteriskExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => 1;
    }
}
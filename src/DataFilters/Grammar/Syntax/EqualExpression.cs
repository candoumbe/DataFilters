using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// A <see cref="FilterExpression"/> obtained when parsing a <c>=</c> token.
    /// </summary>
    public sealed class EqualExpression : FilterExpression, IEquatable<EqualExpression>
    {
        ///<inheritdoc/>
        public bool Equals(EqualExpression other) => other is not null;

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as EqualExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => nameof(EqualExpression).GetHashCode();
    }
}
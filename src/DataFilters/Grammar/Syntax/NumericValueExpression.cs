
using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Wraps a string that represents a numeric value of some sort
    /// </summary>
    /// <remarks>
    /// Builds a new <see cref="NumericValueExpression"/> instance that can wrap a numeric value of some sort
    /// </remarks>
    /// <param name="value"></param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/></exception>
    public class NumericValueExpression(string value) : ConstantValueExpression(value), IEquatable<NumericValueExpression>, IBoundaryExpression
    {
        ///<inheritdoc/>
        public override string EscapedParseableString => Value;

        ///<inheritdoc/>
        public virtual bool Equals(NumericValueExpression other) => Value.Equals(other?.Value);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other) => other switch
        {
            ConstantValueExpression constant => base.Equals(constant),
            ISimplifiable simplifiable => Equals(simplifiable.Simplify()),
            _ => false
        };

        ///<inheritdoc/>
        public static bool operator ==(NumericValueExpression left, NumericValueExpression right) => Equals(left, right);

        ///<inheritdoc/>
        public static bool operator !=(NumericValueExpression left, NumericValueExpression right) => !(left == right);

        ///<inheritdoc/>
        public override bool Equals(object obj) => base.Equals(obj);
    }
}
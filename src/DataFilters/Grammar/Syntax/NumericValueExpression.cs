
using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Wraps a string that represents a numeric value of some sort
    /// </summary>
    public class NumericValueExpression : ConstantValueExpression, IEquatable<NumericValueExpression>, IBoundaryExpression
    {
        /// <summary>
        /// Builds a new <see cref="NumericValueExpression"/> instance that can wrap a numeric value of some sort
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/></exception>
        public NumericValueExpression(string value) : base(value)
        { }

        ///<inheritdoc/>
        public override string EscapedParseableString => Value;

        ///<inheritdoc/>
        public virtual bool Equals(NumericValueExpression other) => Value.Equals(other?.Value);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();


        ///<inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other) => other switch
        {
            NumericValueExpression numeric => Equals(numeric),
            ConstantValueExpression constant => Equals(constant),
            ISimplifiable simplifiable => Equals(simplifiable.Simplify()),
            _ => false
        };

        ///<inheritdoc/>
        public static bool operator ==(NumericValueExpression left, NumericValueExpression right) => Equals(left, right);

        ///<inheritdoc/>
        public static bool operator !=(NumericValueExpression left, NumericValueExpression right) => !(left == right);

    }
}
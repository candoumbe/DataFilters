
using System;
using System.Diagnostics;

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
    [DebuggerDisplay("{Value}")]
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
            NumericValueExpression numericValue => Equals(numericValue),
            ConstantValueExpression constant => base.Equals(constant),
            ISimplifiable simplifiable => Equals(simplifiable.Simplify()),
            _ => false
        };

        ///<inheritdoc/>
        public static bool operator ==(NumericValueExpression left, NumericValueExpression right) => Equals(left, right);

        ///<inheritdoc/>
        public static bool operator !=(NumericValueExpression left, NumericValueExpression right) => !(left == right);

        ///<inheritdoc/>
        public override bool Equals(object obj)  =>
            obj switch
            {
                StringValueExpression stringValue => Value.Equals(stringValue.Value),
                not null => ReferenceEquals(this, obj) || Equals(obj as NumericValueExpression),
                _ => false
            };
    }
}
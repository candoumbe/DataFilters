
using System;
using System.Diagnostics;
using Ardalis.GuardClauses;
using Candoumbe.Types.Strings;
using DataFilters.ValueObjects;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Wraps a string that represents a numeric value of some sort
    /// </summary>
    /// <remarks>
    /// Builds a new <see cref="NumericValueExpression"/> instance that can wrap a numeric value of some sort
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/></exception>
    [DebuggerDisplay("{Value}")]
    public class NumericValueExpression : ConstantValueExpression, IEquatable<NumericValueExpression>, IBoundaryExpression
    {
        internal NumericValueExpression(StringSegmentLinkedList value) : base(value)
        {
        }

        /// <summary>
        /// Wraps a string that represents a numeric value of some sort
        /// </summary>
        /// <remarks>
        /// Builds a new <see cref="NumericValueExpression"/> instance that can wrap a numeric value of some sort
        /// </remarks>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/></exception>
        public NumericValueExpression(string value) : base(value switch
        {
            null => throw new ArgumentNullException(nameof(value)),
            {Length: 0} => throw new ArgumentOutOfRangeException(nameof(value)),
            _ => value
        })
        {
        }

        ///<inheritdoc/>
        public override EscapedString EscapedParseableString => EscapedString.From(Value.ToStringValue());

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
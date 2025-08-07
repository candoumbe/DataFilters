using System;
using Candoumbe.MiscUtilities.Comparers;
using Candoumbe.Types.Strings;
using Microsoft.Extensions.Primitives;

namespace DataFilters.Grammar.Syntax;
/// <summary>
/// An expression that holds a constant value
/// </summary>
public abstract class ConstantValueExpression : FilterExpression, IEquatable<ConstantValueExpression>
{
    /// <summary>
    /// An optimized storage of the current value holds by this instance
    /// </summary>
    public StringSegmentLinkedList Value { get; }

    /// <summary>
    /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">raw unescaped value.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/> or <paramref name="value"/> is not currently supported.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    internal ConstantValueExpression(StringSegment value) : this(new StringSegmentLinkedList(value))
        {}

        internal ConstantValueExpression(StringSegmentLinkedList value)
        {
        Value = value;
    }

    ///<inheritdoc/>
    public virtual bool Equals(ConstantValueExpression other)
            => other is not null && Value.Equals(other.Value, CharComparer.Ordinal);

    ///<inheritdoc/>
    public override bool Equals(object obj) => ReferenceEquals(this, obj) || Equals(obj as ConstantValueExpression);

    ///<inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    ///<inheritdoc/>
    public override double Complexity => 1;

    ///<inheritdoc/>
    public static bool operator ==(ConstantValueExpression left, ConstantValueExpression right)
        => left?.Equals(right) ?? false;

    ///<inheritdoc/>
    public static bool operator !=(ConstantValueExpression left, ConstantValueExpression right)
        => !(left == right);

        /// <summary>
        /// Combines <see cref="ConstantValueExpression"/> and <see cref="EndsWithExpression"/> into a <see cref="AndExpression"/>.
        /// </summary>
        /// <param name="left">a <see cref="ConstantValueExpression"/></param>
        /// <param name="right">a <see cref="EndsWithExpression"/></param>
        /// <returns><see cref="AndExpression"/></returns>
        public static AndExpression operator +(ConstantValueExpression left, EndsWithExpression right) => new StartsWithExpression(left.Value) & right;
}
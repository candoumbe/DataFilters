﻿namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// An expression that holds a constant value
    /// </summary>
    public abstract class ConstantValueExpression : FilterExpression, IEquatable<ConstantValueExpression>
    {
        /// <summary>
        /// Gets the "raw" value hold by the current instance.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">raw unescaped value.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/> or <paramref name="value"/> is not currently supported.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        protected ConstantValueExpression(string value)
        {
            Value = value switch
            {
                null => throw new ArgumentNullException(nameof(value)),
                { Length: 0 } => throw new ArgumentOutOfRangeException(nameof(value)),
                _ => value
            };
        }

        ///<inheritdoc/>
        public virtual bool Equals(ConstantValueExpression other) => Equals(Value, other?.Value);

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
}
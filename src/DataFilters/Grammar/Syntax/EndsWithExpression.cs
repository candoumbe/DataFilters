using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// A <see cref="FilterExpression"/> that holds a string value
    /// </summary>
    public sealed class EndsWithExpression : FilterExpression, IEquatable<EndsWithExpression>
    {
        /// <summary>
        /// The value of the expression
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Builds a new <see cref="EndsWithExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/>'s length is <c>0</c>.</exception>
        public EndsWithExpression(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            Value = value;
        }

        ///<inheritdoc/>
        public bool Equals(EndsWithExpression other) => Value == other?.Value;

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as EndsWithExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => $"{ GetType().Name } : {nameof(Value)} -> '{Value}'";

        ///<inheritdoc/>
        public override double Complexity => 1.5;
    }
}
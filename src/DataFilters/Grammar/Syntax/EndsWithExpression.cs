using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// An expression that holds a string value
    /// </summary>
    public class EndsWithExpression : FilterExpression, IEquatable<EndsWithExpression>
    {
        public string Value { get; }

        /// <summary>
        /// Builds a new <see cref="EndsWithExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
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

        public bool Equals(EndsWithExpression other) => Value == other?.Value;

        public override bool Equals(object obj) => Equals(obj as EndsWithExpression);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => $"{ GetType().Name } : {nameof(Value)} -> {Value}";
    }
}
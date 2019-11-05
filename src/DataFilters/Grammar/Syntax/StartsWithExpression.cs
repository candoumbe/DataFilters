using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// An expression that holds a string value
    /// </summary>
    public class StartsWithExpression : FilterExpression, IEquatable<StartsWithExpression>
    {
        public string Value { get; }

        /// <summary>
        /// Builds a new <see cref="StartsWithExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="value"/> is <c>null</c></exception>
        public StartsWithExpression(string value)
        {
            if (value == string.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public bool Equals(StartsWithExpression other) => Value == other?.Value;

        public override bool Equals(object obj) => Equals(obj as StartsWithExpression);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => $"{ GetType().Name } : {nameof(Value)} -> {Value}";
    }
}
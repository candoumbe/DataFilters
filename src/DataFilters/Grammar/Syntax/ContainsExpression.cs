using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// An expression that holds a string value
    /// </summary>
    public class ContainsExpression : FilterExpression, IEquatable<ContainsExpression>
    {
        public string Value { get; }

        /// <summary>
        /// Builds a new <see cref="StartsWithExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="value"/> is <c>null</c></exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="value"/> is <c>empty</c></exception>
        public ContainsExpression(string value)
        {
            if (value == string.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public bool Equals(ContainsExpression other) => Value == other?.Value;

        public override bool Equals(object obj) => Equals(obj as ContainsExpression);

        public override int GetHashCode() => Value.GetHashCode();
    }
}
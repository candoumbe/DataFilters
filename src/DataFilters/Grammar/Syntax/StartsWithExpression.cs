namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// A <see cref="FilterExpression"/> that defines a string that starts with a specified <see cref="Value"/>.
    /// </summary>
    public sealed class StartsWithExpression : FilterExpression, IEquatable<StartsWithExpression>
    {
        /// <summary>
        /// Value associated with the expression
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Builds a new <see cref="StartsWithExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="value"/> is <c>null</c></exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="value"/> is <see cref="string.Empty"/>.</exception>
        public StartsWithExpression(string value)
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
        public bool Equals(StartsWithExpression other) => Value == other?.Value;

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as StartsWithExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override string ParseableString => $"{Value}*";
    }
}
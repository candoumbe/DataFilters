namespace DataFilters.Grammar.Syntax
{
    using System;
    using System.Linq;

    using static DataFilters.Grammar.Parsing.FilterTokenizer;

    /// <summary>
    /// A <see cref="FilterExpression"/> that holds a string value
    /// </summary>
    public sealed class EndsWithExpression : FilterExpression, IEquatable<EndsWithExpression>
    {
        /// <summary>
        /// The value of the expression
        /// </summary>
        public string Value { get; }

        private readonly Lazy<string> _lazyParseableString;

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

            _lazyParseableString = new(() => {
                string parseableString = Value;
                if (parseableString.Any(chr => SpecialCharacters.Contains(chr)))
                {
                    parseableString = $"*{string.Concat(Value.Select(chr => char.IsWhiteSpace(chr) || SpecialCharacters.Contains(chr) ? $@"\{chr}" : $"{chr}"))}";
                }

                return parseableString;
            });
        }

        ///<inheritdoc/>
        public bool Equals(EndsWithExpression other) => Value == other?.Value;

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as EndsWithExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override string ParseableString => $"*{Value}";

        ///<inheritdoc/>
        public override double Complexity => 1.5;
    }
}
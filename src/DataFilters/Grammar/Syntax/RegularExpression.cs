using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// An expression that holds a all character set of a regular expression value
    /// </summary>
    public class RegularExpression : FilterExpression, IEquatable<RegularExpression>
    {
        /// <summary>
        /// Content of the regular expression.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Builds a new <see cref="RegularExpression"/> instance
        /// </summary>
        /// <param name="value">Content of the regular expression</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public RegularExpression(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));

        public bool Equals(RegularExpression other) => Value == other?.Value;

        public override bool Equals(object obj) => Equals(obj as RegularExpression);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => $"{nameof(RegularExpression)} : Value -> {Value}";
    }
}
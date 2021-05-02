using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// A <see cref="FilterExpression"/> that holds a set of a regular expression as its <see cref="Value"/>.
    /// </summary>
    public sealed class RegularExpression : FilterExpression, IEquatable<RegularExpression>
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

        ///<inheritdoc/>
        public bool Equals(RegularExpression other) => Value == other?.Value;

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as RegularExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => $"{nameof(RegularExpression)} : Value -> {Value}";
    }
}
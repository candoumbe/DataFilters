using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Represents a string value that do not need to be escaped any further.
    /// </summary>
    public sealed record EscapedString
    {
        /// <summary>
        /// The underlying value
        /// </summary>
        public string Value { get; }

        private EscapedString(string value) => Value = value;

        /// <summary>
        /// Represents an empty escaped string.
        /// </summary>
        public static readonly EscapedString Empty = new(string.Empty);

        /// <summary>
        /// Escape the specified <see cref="string"/> value.
        /// </summary>
        /// <param name="value">The string value to escape</param>
        /// <returns>The "escaped" version of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException"> when <paramref name="value"/> is <see langword="null"/> </exception>
        public static EscapedString From(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new EscapedString(value);
        }
    }
}
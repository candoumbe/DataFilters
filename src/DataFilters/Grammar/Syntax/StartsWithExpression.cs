namespace DataFilters.Grammar.Syntax
{
    using DataFilters.Grammar.Parsing;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
#if !NETSTANDARD1_3
    using Ardalis.GuardClauses;
#endif

    /// <summary>
    /// A <see cref="FilterExpression"/> that defines a string that starts with a specified <see cref="Value"/>.
    /// </summary>
    public sealed class StartsWithExpression : FilterExpression, IEquatable<StartsWithExpression>
    {
        /// <summary>
        /// Value associated with the expression
        /// </summary>
        public string Value { get; }

        private readonly Lazy<string> _lazyEscapedParseableString;

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

            _lazyEscapedParseableString = new(() =>
            {
                // The length of the final parseable string in worst cases scenario will double (1 backlash + the escaped character)
                // Also we need an extra position for the final '*' that will be append in all cases
                bool requireEscapingCharacters = Value.AtLeastOnce(chr => FilterTokenizer.SpecialCharacters.Contains(chr));
                StringBuilder parseableString;

                if (requireEscapingCharacters)
                {
                    parseableString = new((Value.Length * 2) + 1);
                    foreach (char chr in Value)
                    {
                        if (FilterTokenizer.SpecialCharacters.Contains(chr))
                        {
                            parseableString = parseableString.Append('\\');
                        }
                        parseableString = parseableString.Append(chr);
                    }
                }
                else
                {
                    parseableString = new(Value, Value.Length + 1);
                }

                return parseableString.Append('*').ToString();
            });
        }

        /// <summary>
        /// Builds a new <see cref="StartsWithExpression"/> that holds the specified <paramref name="text"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
        public StartsWithExpression(TextExpression text)
#if !NETSTANDARD1_3
            : this (Guard.Against.Null(text, nameof(text)).OriginalString)
        {
            _lazyEscapedParseableString = new(() => $"{text.EscapedParseableString}*");
        }
#else
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            _lazyEscapedParseableString = new(() => $"{text.EscapedParseableString}*");
        }
#endif

        ///<inheritdoc/>
        public bool Equals(StartsWithExpression other) => Value == other?.Value;

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as StartsWithExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => OriginalString;

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyEscapedParseableString.Value;
    }
}
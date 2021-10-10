namespace DataFilters.Grammar.Syntax
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using static DataFilters.Grammar.Parsing.FilterTokenizer;

#if !NETSTANDARD1_3
    using Ardalis.GuardClauses;
#endif

    /// <summary>
    /// A <see cref="FilterExpression"/> that holds a string value
    /// </summary>
    public sealed class EndsWithExpression : FilterExpression, IEquatable<EndsWithExpression>
    {
        /// <summary>
        /// The value of the expression
        /// </summary>
        public string Value { get; }

        private readonly Lazy<string> _lazyEscapedParseableString;

        /// <summary>
        /// Builds a new <see cref="EndsWithExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
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

            _lazyEscapedParseableString = new(() =>
            {
                // The length of the final parseable string in worst cases scenario will double (1 backlash + the escaped character)
                // Also we need an extra position for the final '*' that will be append in all cases
                bool requireEscapingCharacters = value.AtLeastOnce(chr => SpecialCharacters.Contains(chr));
                StringBuilder parseableString;

                if (requireEscapingCharacters)
                {
                    parseableString = new((value.Length * 2) + 1);
                    foreach (char chr in value)
                    {
                        if (SpecialCharacters.Contains(chr))
                        {
                            parseableString = parseableString.Append('\\');
                        }
                        parseableString = parseableString.Append(chr);
                    }
                }
                else
                {
                    parseableString = new(value, value.Length + 1);
                }

                return parseableString.Insert(0, '*').ToString();
            });
        }

        /// <summary>
        /// Builds a new <see cref="EndsWithExpression"/> that holds the specified <paramref name="text"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
        public EndsWithExpression(TextExpression text)
#if !NETSTANDARD1_3
            : this (Guard.Against.Null(text, nameof(text)).OriginalString)
        {
            _lazyEscapedParseableString = new(() => $"*{text.EscapedParseableString}");
        }
#else
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            _lazyEscapedParseableString = new(() => $"*{text.EscapedParseableString}");
        }
#endif

        ///<inheritdoc/>
        public bool Equals(EndsWithExpression other) => Value == other?.Value;

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as EndsWithExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyEscapedParseableString.Value;

        ///<inheritdoc/>
        public override double Complexity => 1.5;
    }
}
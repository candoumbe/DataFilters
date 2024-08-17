namespace DataFilters.Grammar.Syntax
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
#if !NETSTANDARD1_3
    using Ardalis.GuardClauses;
#endif

    using static DataFilters.Grammar.Parsing.FilterTokenizer;

    /// <summary>
    /// A <see cref="FilterExpression"/> that holds a string value
    /// </summary>
    public sealed class ContainsExpression : FilterExpression, IEquatable<ContainsExpression>, IFormattable
    {
        /// <summary>
        /// The value that was between two <see cref="AsteriskExpression"/>
        /// </summary>
        public string Value { get; }

        private readonly Lazy<string> _lazyEscapedParseableString;

        /// <summary>
        /// Builds a new <see cref="ContainsExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="value"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="value"/> is <c>empty</c></exception>
        public ContainsExpression(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(value)} cannot be empty");
            }

            Value = value;

            _lazyEscapedParseableString = new Lazy<string>(() =>
            {
                // The length of the final parseable string in worst case scenarios will double (1 backlash for each character of the original input)
                // Also we need two extra positions for '*' that will be prepended and appended in all cases
                bool requireEscapingCharacters = Value.AtLeastOnce(chr => SpecialCharacters.Contains(chr));
                StringBuilder parseableString;

                if (requireEscapingCharacters)
                {
                    parseableString = new StringBuilder(( Value.Length * 2 ) + 2);
                    foreach (char chr in Value)
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
                    parseableString = new StringBuilder(Value, Value.Length + 2);
                }

                return parseableString.Insert(0, "*").Append('*').ToString();
            });
        }

        /// <summary>
        /// Builds a new <see cref="ContainsExpression"/> that holds the specified <paramref name="text"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
        public ContainsExpression(TextExpression text)
#if !NETSTANDARD1_3
            : this(Guard.Against.Null(text, nameof(text)).OriginalString)
        {
            _lazyEscapedParseableString = new(() => $"*{text.EscapedParseableString}*");
        }
#else
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            _lazyEscapedParseableString = new(() => $"*{text.EscapedParseableString}*");
        }
#endif

        ///<inheritdoc/>
        public override double Complexity => 1.5;

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as ContainsExpression);

        ///<inheritdoc/>
        public bool Equals(ContainsExpression other) => Value == other?.Value;

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyEscapedParseableString.Value;

        ///<inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other)
            => other switch
            {
                ContainsExpression contains => Equals(contains),
                GroupExpression { Expression: var innerExpression } => innerExpression.IsEquivalentTo(this),
                ISimplifiable simplifiable => simplifiable.Simplify().IsEquivalentTo(this),
                _ => false
            };
    }
}

namespace DataFilters.Grammar.Syntax
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using static DataFilters.Grammar.Parsing.FilterTokenizer;

    /// <summary>
    /// Wraps a string that represents a constant string value
    /// </summary>
    public class StringValueExpression : ConstantValueExpression, IEquatable<StringValueExpression>
    {
        private readonly Lazy<string> _lazyParseableString;

        /// <summary>
        /// Builds a new <see cref="StringValueExpression"/> instance that can wrap a string value
        /// </summary>
        /// <param name="value">value of the expression.</param>
        /// <remarks>
        /// The <see cref="EscapedParseableString"/> property automatically escapes <see cref="SpecialCharacters"/> from <paramref name="value"/>.
        /// </remarks>
        public StringValueExpression(string value) : base(value)
        {
            _lazyParseableString = new(() =>
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
                    parseableString = new(value);
                }

                return parseableString.ToString();
            });
        }

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyParseableString.Value;

        ///<inheritdoc/>
        public override string OriginalString => Value;

        ///<inheritdoc/>
        public virtual bool Equals(StringValueExpression other) => Equals(Value, other?.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) => obj switch {
            StringValueExpression stringValue => Equals(stringValue),
            ISimplifiable simplifiable => Equals(simplifiable.Simplify() as StringValueExpression),
            _ => false
        };
        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override double Complexity => 1;

        ///<inheritdoc/>
        public static bool operator ==(StringValueExpression left, StringValueExpression right)
            => (left is null && right is null) || (left?.Equals(right) ?? false);

        ///<inheritdoc/>
        public static bool operator !=(StringValueExpression left, StringValueExpression right)
            => !(left == right);
    }
}
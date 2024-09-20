
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataFilters.Grammar.Parsing;

namespace DataFilters.Grammar.Syntax
{
    using static FilterTokenizer;

    /// <summary>
    /// Wraps a string that represents a constant string value
    /// </summary>
    /// <remarks>
    /// Builds a new <see cref="StringValueExpression"/> instance that can wrap a string value
    /// </remarks>
    /// <param name="value">value of the expression.</param>
    /// <remarks>
    /// The <see cref="EscapedParseableString"/> property automatically escapes <see cref="SpecialCharacters"/> from <paramref name="value"/>.
    /// </remarks>
    public class StringValueExpression(string value) : ConstantValueExpression(value), IEquatable<StringValueExpression>
    {
        private readonly Lazy<string> _lazyParseableString = new(() =>
            {
                // The length of the final parseable string in worst cases scenario will double (1 backlash + the escaped character)
                bool requireEscapingCharacters = value.AtLeastOnce(chr => SpecialCharacters.Contains(chr));
                StringBuilder parseableString;

                if (requireEscapingCharacters)
                {
                    parseableString = new StringBuilder(value.Length * 2);
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
                    parseableString = new StringBuilder(value);
                }

                return parseableString.ToString();
            });

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyParseableString.Value;

        ///<inheritdoc/>
        public override string OriginalString => Value;

        ///<inheritdoc/>
        public virtual bool Equals(StringValueExpression other) => Equals(Value, other?.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) =>
            obj switch
            {
                NumericValueExpression numericValue => Value.Equals(numericValue.Value),
                not null => ReferenceEquals(this, obj) || Equals(obj as StringValueExpression),
                _ => false
            };

        /// <inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other)
            => other switch
            {
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
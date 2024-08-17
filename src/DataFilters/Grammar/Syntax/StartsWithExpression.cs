namespace DataFilters.Grammar.Syntax
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DataFilters.Grammar.Parsing;
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
        /// <param name="value">The value associated with the expression</param>
        /// <exception cref="ArgumentNullException">if <paramref name="value"/> is <c>null</c></exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="value"/> is <see cref="string.Empty"/>.</exception>
        /// <remarks>
        /// The constructor will take care of escaping all specific characters of <paramref name="value"/>
        /// </remarks>
        public StartsWithExpression(string value)
        {
            Value = value switch
            {
                null => throw new ArgumentNullException(nameof(value)),
                {Length: 0} => throw new ArgumentOutOfRangeException(nameof(value)),
                _ => value
            };

            _lazyEscapedParseableString = new Lazy<string>(() =>
            {
                // The length of the final parseable string in worst cases scenario will double (1 backlash + the escaped character)
                // Also we need an extra position for the final '*' that will be append in all cases
                bool requireEscapingCharacters = Value.AtLeastOnce(chr => FilterTokenizer.SpecialCharacters.Contains(chr));
                StringBuilder escapedParseableString;

                if (requireEscapingCharacters)
                {
                    escapedParseableString = new StringBuilder((Value.Length * 2) + 1);
                    foreach (char chr in Value)
                    {
                        if (FilterTokenizer.SpecialCharacters.Contains(chr))
                        {
                            escapedParseableString = escapedParseableString.Append('\\');
                        }

                        escapedParseableString = escapedParseableString.Append(chr);
                    }
                }
                else
                {
                    escapedParseableString = new StringBuilder(Value, Value.Length + 1);
                }

                return escapedParseableString.Append('*').ToString();
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
            _lazyEscapedParseableString = new Lazy<string>(() => $"{text.EscapedParseableString}*");
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
        public override string EscapedParseableString => _lazyEscapedParseableString.Value;

        /// <summary>
        /// Combines the specified <paramref name="left"/> and <paramref name="right"/> <see cref="StartsWithExpression"/>s into a new <see cref="AndExpression"/>.
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>a <see cref="AndExpression"/> whose <see cref="BinaryFilterExpression.Left"/> is <paramref name="left"/> and <see cref="BinaryFilterExpression.Right"/> is
        /// <paramref name="right"/></returns>
        /// <exception cref="ArgumentNullException">if either <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.</exception>
        public static AndExpression operator +(StartsWithExpression left, EndsWithExpression right) => new AndExpression(left, right);

        /// <summary>
        /// Combines the specified <paramref name="left"/> and <paramref name="right"/>
        /// </summary>
        /// <param name="left">the left operand</param>
        /// <param name="right">the right operand</param>
        /// <returns>
        /// A <see cref="OneOfExpression"/> that can either :
        /// <list type="bullet">
        ///     <item>exactly matches the concatenation of <paramref name="left"/>'s value and <paramref name="right"/>'s value.</item>
        ///     <item>exactly starts with the concatenation of <paramref name="left"/>'s value and <paramref name="right"/>'s value.</item>
        ///     <item>exactly starts with <paramref name="left"/>'s value and contains <paramref name="right"/>'s value.</item>
        /// </list>
        /// </returns>
        public static OneOfExpression operator +(StartsWithExpression left, StartsWithExpression right) => new OneOfExpression(new StringValueExpression(left.Value + right.Value), new StartsWithExpression(left.Value + right.Value), new AndExpression(left, new ContainsExpression(right.Value)));

        /// <summary>
        /// Combines the specified <paramref name="left"/> and <paramref name="right"/>
        /// </summary>
        /// <param name="left">the left operand</param>
        /// <param name="right">the right operand</param>
        /// <returns>
        /// A <see cref="OneOfExpression"/> that can either :
        /// <list type="bullet">
        ///     <item>exactly matches the concatenation of <paramref name="left"/>'s value and <paramref name="right"/>'s value.</item>
        ///     <item>exactly starts with the concatenation of <paramref name="left"/>'s value and <paramref name="right"/>'s value.</item>
        ///     <item>exactly starts with <paramref name="left"/>'s value and contains <paramref name="right"/>'s value.</item>
        /// </list>
        /// </returns>
        /// <example>
        /// bat*,*man* &lt;==&gt; batman* | bat*man* | batman
        /// </example>
        public static OneOfExpression operator +(StartsWithExpression left, ContainsExpression right)
        {
            string value = $"{left.Value}{right.Value}";

            return new OneOfExpression(new StartsWithExpression(value), new AndExpression(left, right), new StringValueExpression(value));
        }

        /// <summary>
        /// Combines the specified <paramref name="left"/> and <paramref name="right"/>
        /// </summary>
        /// <param name="left">the left operand</param>
        /// <param name="right">the right operand</param>
        /// <returns>
        /// A <see cref="AndExpression"/> that can match any <see langword="string"/> that starts with <paramref name="left"/>
        /// and ends with <paramref name="right"/>.
        /// </returns>
        public static AndExpression operator +(StartsWithExpression left, StringValueExpression right) => left + new EndsWithExpression(right.Value);

        /// <summary>
        /// Combines the specified <paramref name="left"/> and <paramref name="right"/> into and <see cref="AndExpression"/> expression
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static AndExpression operator &(StartsWithExpression left, StringValueExpression right) => left + right;

        /// <summary>
        /// Combines the specified <paramref name="left"/> and <paramref name="right"/> into and <see cref="AndExpression"/> expression
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static AndExpression operator &(StartsWithExpression left, EndsWithExpression right) => left + right;

        /// <summary>
        /// Negates the specified <paramref name="expression"/>
        /// </summary>
        /// <param name="expression">The start expression</param>
        /// <returns></returns>
        public static NotExpression operator !(StartsWithExpression expression) => new (expression);
    }
}
using System;
using System.Linq;
using System.Text;
using Candoumbe.Types.Strings;
using DataFilters.Grammar.Parsing;
using Microsoft.Extensions.Primitives;

namespace DataFilters.Grammar.Syntax
{
    using static FilterTokenizer;

#if !NETSTANDARD1_3
    using Ardalis.GuardClauses;
#endif

    /// <summary>
    /// A <see cref="FilterExpression"/> that holds a string value
    /// </summary>
    public sealed class EndsWithExpression : FilterExpression, IEquatable<EndsWithExpression>
    {
        private readonly Lazy<string> _lazyEscapedParseableString;

        /// <summary>
        /// A memory optimized representation of the value hold by the current instance.
        /// </summary>
        public StringSegmentLinkedList Value { get; }

        /// <summary>
        /// Builds a new <see cref="ContainsExpression"/> instance which holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The desired value</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> contains <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="StringSegment.Empty"/>.</exception>
        public EndsWithExpression(StringSegment value) : this(new StringSegmentLinkedList(value))
        {
            if (!value.HasValue)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (StringSegment.IsNullOrEmpty(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        /// <summary>
        /// Builds a new <see cref="EndsWithExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/>'s length is <c>0</c>.</exception>
        public EndsWithExpression(StringSegmentLinkedList value)
        {
            Value = value switch
            {
                { Count: 0 } => throw new ArgumentOutOfRangeException(nameof(value)),
                _ => value
            };

            _lazyEscapedParseableString = new Lazy<string>(() =>
            {
                string escapedValue = Value.Replace(chr => SpecialCharacters.Contains(chr), EscapedSpecialCharacters)
                    .ToStringValue();

                return $"*{escapedValue}";
            });
        }

        /// <summary>
        /// Builds a new <see cref="EndsWithExpression"/> that holds the specified <paramref name="text"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
        public EndsWithExpression(TextExpression text)
#if !NETSTANDARD1_3
            : this(new StringSegmentLinkedList( Guard.Against.Null(text, nameof(text)).OriginalString ))
        {
            _lazyEscapedParseableString = new Lazy<string>(() => $"*{text.EscapedParseableString}");
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
        public bool Equals(EndsWithExpression other) => Value.Equals(other?.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as EndsWithExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyEscapedParseableString.Value;

        ///<inheritdoc/>
        public override double Complexity => 1.5;

        /// <summary>
        /// Constructs a new <see cref="ContainsExpression"/> by adding an <see cref="AsteriskExpression"/> to a <see cref="EndsWithExpression"/>>.
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="_"></param>
        /// <returns></returns>
        public static ContainsExpression operator +(EndsWithExpression left, AsteriskExpression _) => new(left.Value);
    }
}
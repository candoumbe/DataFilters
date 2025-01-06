using System;
using System.Linq;
using System.Text;
using Candoumbe.Types.Strings;
using DataFilters.Grammar.Parsing;
using DataFilters.ValueObjects;
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
        private readonly Lazy<EscapedString> _lazyEscapedParseableString;

        /// <summary>
        /// A memory optimized representation of the value hold by the current instance.
        /// </summary>
        public StringSegmentLinkedList Value { get; }

        /// <summary>
        /// Builds a new <see cref="ContainsExpression"/> instance which holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The desired value</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> contains <see langword="null"/>.</exception>
        public EndsWithExpression(EscapedString value)
        {
            Value = new StringSegmentLinkedList(Guard.Against.Null(value, nameof(value)).Value);
            _lazyEscapedParseableString = new Lazy<EscapedString>(() => EscapedString.From($"*{value}"));
        }

        /// <summary>
        /// Builds a new <see cref="EndsWithExpression"/> which value is based on specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="value"/> is <see langword="null"/>.</exception>
        public EndsWithExpression(ConstantValueExpression value)
        {
            (_lazyEscapedParseableString, Value) = value switch
            {
                TextExpression text => ( new Lazy<EscapedString>(() => EscapedString.From($@"*""{text.EscapedParseableString}""")), text.Value ),
                not null => ( new Lazy<EscapedString>(() => value.EscapedParseableString), value.Value ),
                _ => throw new ArgumentNullException(nameof(value))
            };
        }

        /// <summary>
        /// Builds a new <see cref="ContainsExpression"/> instance which holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The desired value</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> contains <see langword="null"/>.</exception>
        public EndsWithExpression(RawString value): this(new StringSegmentLinkedList(Guard.Against.Null(value, nameof(value)).Value), false)
        {}

        /// <summary>
        /// Builds a new <see cref="EndsWithExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="escaped"></param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/>'s length is <c>0</c>.</exception>
        internal EndsWithExpression(StringSegmentLinkedList value, bool escaped)
        {
            Value = value switch
            {
                { Count: 0 } => throw new ArgumentOutOfRangeException(nameof(value)),
                _ => value
            };

            _lazyEscapedParseableString = escaped
                ? new Lazy<EscapedString>(() => EscapedString.From($"*{Value.ToStringValue()}"))
                : new Lazy<EscapedString>(() =>
                {
                    string escapedValue = Value.Replace(chr => SpecialCharacters.Contains(chr), EscapedSpecialCharacters)
                        .ToStringValue();

                    return EscapedString.From($"*{escapedValue}");
                });
        }

        /// <summary>
        /// Builds a new <see cref="EndsWithExpression"/> that holds the specified <paramref name="text"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
        public EndsWithExpression(TextExpression text)
            : this(Guard.Against.Null(text, nameof(text)).EscapedParseableString)
        {
            _lazyEscapedParseableString = new Lazy<EscapedString>(() => EscapedString.From($"*{text.EscapedParseableString}"));
        }

        ///<inheritdoc/>
        public bool Equals(EndsWithExpression other) => Value.Equals(other?.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as EndsWithExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override EscapedString EscapedParseableString => _lazyEscapedParseableString.Value;

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
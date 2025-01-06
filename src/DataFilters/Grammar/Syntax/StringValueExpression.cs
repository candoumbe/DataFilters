
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Candoumbe.Types.Strings;
using DataFilters.Grammar.Parsing;
using DataFilters.ValueObjects;
using Microsoft.Extensions.Primitives;

namespace DataFilters.Grammar.Syntax
{
    using static FilterTokenizer;

    /// <summary>
    /// Wraps a string that represents a constant string value
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <remarks>
    /// The <see cref="EscapedParseableString"/> property automatically escapes <see cref="SpecialCharacters"/> from <paramref name="value"/>.
    /// </remarks>
    public class StringValueExpression : ConstantValueExpression, IEquatable<StringValueExpression>
    {
        private readonly Lazy<EscapedString> _lazyParseableString;
        private readonly Lazy<string> _lazyOriginalString;

        /// <summary>
        /// Builds a new <see cref="StringValueExpression"/> instance that can wrap a string value
        /// </summary>
        /// <param name="value">value of the expression.</param>
        public StringValueExpression(StringSegment value) : this(new StringSegmentLinkedList(value))
        {
            if (!value.HasValue)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        /// <summary>
        /// Builds a new <see cref="StringValueExpression"/> instance that can wrap a string value
        /// </summary>
        /// <param name="value">value of the expression.</param>
        public StringValueExpression(StringSegmentLinkedList value) : base(value)
        {
            _lazyOriginalString = new Lazy<string>(value.ToStringValue);
            _lazyParseableString = new Lazy<EscapedString>(() => EscapedString.From(Value.Replace(chr => SpecialCharacters.Contains(chr),
                                          EscapedSpecialCharacters).ToStringValue()));
        }

        ///<inheritdoc/>
        public override EscapedString EscapedParseableString => _lazyParseableString.Value;

        ///<inheritdoc/>
        public virtual bool Equals(StringValueExpression other) => Equals(_lazyOriginalString.Value, other?._lazyOriginalString.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) =>
            obj switch
            {
                NumericValueExpression numericValue => EscapedParseableString.Equals(numericValue.EscapedParseableString),
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
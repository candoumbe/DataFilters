
using System;
using Candoumbe.Types.Strings;
using DataFilters.ValueObjects;
using Microsoft.Extensions.Primitives;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Wraps a string that represents a <see cref="System.Guid"/>
    /// </summary>
    public class GuidValueExpression : ConstantValueExpression
    {
        private readonly Lazy<EscapedString> _lazyParseableString;

        /// <summary>
        /// Builds a new <see cref="GuidValueExpression"/> instance that can wrap a <see cref="System.Guid"/>
        /// </summary>
        /// <param name="value"></param>
        public GuidValueExpression(StringSegment value): base(value)
        {
            _lazyParseableString = new Lazy<EscapedString>(() => EscapedString.From(value.Value));
        }

        internal GuidValueExpression(StringSegmentLinkedList value): base(value)
        {
            _lazyParseableString = new Lazy<EscapedString>(() => EscapedString.From(value.ToStringValue()));
        }

        /// <inheritdoc />
        public override EscapedString EscapedParseableString => _lazyParseableString.Value;

        /// <inheritdoc />
        public override string ToString() => EscapedParseableString.Value;
    }
}
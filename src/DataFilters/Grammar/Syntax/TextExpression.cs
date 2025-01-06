using Candoumbe.Types.Strings;
using DataFilters.Grammar.Parsing;
using DataFilters.ValueObjects;
using Microsoft.Extensions.Primitives;

namespace DataFilters.Grammar.Syntax
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// An expression that holds a string value "as is".
    /// </summary>
    public class TextExpression : StringValueExpression, IEquatable<TextExpression>
    {
        private readonly Lazy<EscapedString> _lazyEscapedParseableString;
        private readonly StringSegmentLinkedList _stringSegments;
        private readonly Lazy<string> _lazyOriginalString;

        /// <summary>
        /// Builds a new <see cref="TextExpression"/> instance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <paramref name="value"/> should neither start nor end with &quot; unless these quotes are part of the raw text expression.
        /// More specifically, all quotes inside <paramref name="value"/> will be escaped.
        /// </para>
        /// <list type="table">
        /// <listheader>
        ///     <term>Input</term>
        ///     <term>Outputs</term>
        /// </listheader>
        /// <item>
        ///     <term><c>foo</c></term>
        ///     <description>
        ///         <list type="bullet">
        ///             <item>
        ///                 <term><see cref="ConstantValueExpression.Value"/></term>
        ///                 <description><c>foo</c></description>
        ///             </item>
        ///             <item>
        ///                 <term><see cref="OriginalString"/></term>
        ///                 <description><c>foo</c></description>
        ///             </item>
        ///             <item>
        ///                 <term><see cref="EscapedParseableString"/></term>
        ///                 <description><c>"foo"</c></description>
        ///             </item>
        ///         </list>
        ///     </description>
        /// </item>
        /// <item>
        ///     <term><c>"bar"</c></term>
        ///     <description>
        ///         <list type="bullet">
        ///             <item>
        ///                 <term><see cref="ConstantValueExpression.Value"/></term>
        ///                 <description><c>"bar"</c></description>
        ///             </item>
        ///             <item>
        ///                 <term><see cref="OriginalString"/></term>
        ///                 <description><c>"bar"</c></description>
        ///             </item>
        ///             <item>
        ///                 <term><see cref="EscapedParseableString"/></term>
        ///                 <description><c>"\"bar\""</c></description>
        ///             </item>
        ///         </list>
        ///     </description>
        /// </item>
        /// <item>
        ///     <term><c>foo"bar</c></term>
        ///     <description>
        ///         <list type="bullet">
        ///             <item>
        ///                 <term><see cref="ConstantValueExpression.Value"/></term>
        ///                 <description><c>foo"bar</c></description>
        ///             </item>
        ///             <item>
        ///                 <term><see cref="OriginalString"/></term>
        ///                 <description><c>foo"bar</c></description>
        ///             </item>
        ///             <item>
        ///                 <term><see cref="EscapedParseableString"/></term>
        ///                 <description><c>"foo\"bar"</c></description>
        ///             </item>
        ///         </list>
        ///     </description> 
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="value">Value of the expression.</param>
        public TextExpression(StringSegment value) : this(new StringSegmentLinkedList(value))
        {
        }

        internal TextExpression(StringSegmentLinkedList value) : base(value)
        {
            _lazyOriginalString = new Lazy<string>(value.ToStringValue);
            _lazyEscapedParseableString = new Lazy<EscapedString>(() =>
            {
                StringBuilder sb = new (value: FilterTokenizer.DoubleQuote.ToString());
                sb.Append(value.Replace(chr => chr is FilterTokenizer.BackSlash or FilterTokenizer.DoubleQuote, new Dictionary<char, ReadOnlyMemory<char>>
                    {
                        [FilterTokenizer.BackSlash] = FilterTokenizer.EscapedSpecialCharacters[FilterTokenizer.BackSlash],
                        [FilterTokenizer.DoubleQuote] = FilterTokenizer.EscapedSpecialCharacters[FilterTokenizer.DoubleQuote]
                    })
                    .ToStringValue());

                return EscapedString.From(sb.Append(FilterTokenizer.DoubleQuote).ToString());
            });
        }

        ///<inheritdoc/>
        public override EscapedString EscapedParseableString => _lazyEscapedParseableString.Value;

        ///<inheritdoc/>
        public virtual bool Equals(TextExpression other) => _lazyOriginalString.Value.Equals(other?._lazyOriginalString.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as TextExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => _lazyOriginalString.GetHashCode();
    }
}
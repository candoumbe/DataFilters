﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Candoumbe.Types.Strings;
using DataFilters.Grammar.Parsing;
using Microsoft.Extensions.Primitives;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// A <see cref="FilterExpression"/> that holds a string value
    /// </summary>
    public sealed class ContainsExpression : FilterExpression, IEquatable<ContainsExpression>
    {
        /// <summary>
        /// The value that was between two <see cref="AsteriskExpression"/>
        /// </summary>
        public StringSegmentLinkedList Value { get; }

        private readonly Lazy<string> _lazyEscapedParseableString;

        /// <summary>
        /// Builds a new <see cref="ContainsExpression"/> instance which holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The desired value</param>
        public ContainsExpression(StringSegment value) : this(new StringSegmentLinkedList(value))
        {
            if (value.Value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.Length is 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        /// <summary>
        /// Builds a new <see cref="ContainsExpression"/> that holds the specified <paramref name="text"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
        public ContainsExpression(TextExpression text)
            : this(text switch
            {
                null => throw new ArgumentNullException(nameof(text)),
                _ =>   text.OriginalString
            })
        {
            _lazyEscapedParseableString = new Lazy<string>(() =>
            {
                string escapedValue = text.Value.Replace(chr => chr is FilterTokenizer.BackSlash or FilterTokenizer.DoubleQuote,
                        new Dictionary<char, ReadOnlyMemory<char>> { [FilterTokenizer.BackSlash] = FilterTokenizer.EscapedSpecialCharacters[FilterTokenizer.BackSlash], [FilterTokenizer.DoubleQuote] = FilterTokenizer.EscapedSpecialCharacters[FilterTokenizer.DoubleQuote] })
                    .ToStringValue();

                return $@"*""{escapedValue}""*";
            });
        }

        private static Lazy<string> BuildLazyEscapedParseableString(StringSegmentLinkedList value)
            => new Lazy<string>(() =>
            {
                string escapedValue = value.Replace(chr => FilterTokenizer.SpecialCharacters.Contains(chr),
                        FilterTokenizer.EscapedSpecialCharacters)
                    .ToStringValue();

                return $"*{escapedValue}*";
            });

        /// <summary>
        /// Builds a new <see cref="ContainsExpression"/> that holds the specified <paramref name="segmentList"/>.
        /// </summary>
        /// <param name="segmentList">An optimized representation of the value to hold</param>
        public ContainsExpression(StringSegmentLinkedList segmentList)
        {
            Value = segmentList;
            _lazyEscapedParseableString = BuildLazyEscapedParseableString(Value);
        }

        ///<inheritdoc/>
        public override double Complexity => 1.5;

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as ContainsExpression);

        ///<inheritdoc/>
        public bool Equals(ContainsExpression other) => other is not null && Value.Equals(other.Value);

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
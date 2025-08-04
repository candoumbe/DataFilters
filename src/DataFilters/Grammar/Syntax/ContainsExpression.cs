using System;
using System.Linq;
using Candoumbe.Types.Strings;
using DataFilters.Grammar.Parsing;
using Microsoft.Extensions.Primitives;

namespace DataFilters.Grammar.Syntax;

/// <summary>
/// A <see cref="FilterExpression"/> that holds a string value.
/// </summary>
public sealed class ContainsExpression : FilterExpression, IEquatable<ContainsExpression>
{
    /// <summary>
    /// The value that was between two <see cref="AsteriskExpression"/>s.
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
    /// Builds a new <see cref="ContainsExpression"/> instance which holds the specified <paramref name="escapedString"/> as value.
    /// </summary>
    /// <param name="escapedString">The value of the expression</param>
    /// <remarks>Use this constructor whenever you don't want the value to be escaped.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="escapedString"/> is <see langword="null"/>.</exception>
    public ContainsExpression(EscapedString escapedString)
    {
        Value = new StringSegmentLinkedList(escapedString.Value);
        _lazyEscapedParseableString = new(() => $"*{escapedString.Value}*");
    }

    /// <summary>
    /// Builds a new <see cref="ContainsExpression"/> that holds the specified <paramref name="text"/>.
    /// </summary>
    /// <param name="text"></param>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    public ContainsExpression(TextExpression text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        Value = new StringSegmentLinkedList(FilterTokenizer.AsteriskString).Append(text.Value).Append(FilterTokenizer.AsteriskString);
        _lazyEscapedParseableString = new Lazy<string>(() => $"*{text.EscapedParseableString}*");
    }

    /// <summary>
    /// Builds a new <see cref="ContainsExpression"/> that holds the specified <paramref name="segmentList"/>.
    /// </summary>
    /// <param name="segmentList">An optimized representation of the value to hold</param>
    public ContainsExpression(StringSegmentLinkedList segmentList)
    {
        Value = segmentList;
        _lazyEscapedParseableString = new Lazy<string>(() =>
                                                       {
                                                           string escapedValue = segmentList.Replace(chr => FilterTokenizer.SpecialCharacters.Contains(chr),
                                                                                                     FilterTokenizer.EscapedSpecialCharacters)
                                                               .ToStringValue();

                                                           return $"*{escapedValue}*";
                                                       });
    }

    ///<inheritdoc/>
    public override double Complexity => 1.5;

    ///<inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as ContainsExpression) || IsEquivalentTo(obj as FilterExpression);

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
            ContainsExpression contains                         => Equals(contains),
            GroupExpression { Expression: var innerExpression } => innerExpression.IsEquivalentTo(this),
            ISimplifiable simplifiable                          => simplifiable.Simplify().IsEquivalentTo(this),
            _                                                   => false
        };
}
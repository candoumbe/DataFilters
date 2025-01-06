using Candoumbe.Types.Strings;
using Microsoft.Extensions.Primitives;
using DataFilters.Grammar.Parsing;

using System;
using System.Linq;
using Ardalis.GuardClauses;
using DataFilters.ValueObjects;

namespace DataFilters.Grammar.Syntax;


/// <summary>
/// A <see cref="FilterExpression"/> that defines a string that starts with a specified <see cref="Value"/>.
/// </summary>
public sealed class StartsWithExpression : FilterExpression, IEquatable<StartsWithExpression>
{
    /// <summary>
    /// Value associated with the expression
    /// </summary>
    public StringSegmentLinkedList Value { get; }

    private readonly Lazy<EscapedString> _lazyEscapedParseableString;

    /// <summary>
    /// Builds a new <see cref="StartsWithExpression"/> instance which holds the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The desired value</param>
    /// <exception cref="ArgumentNullException">value was built against a string that is <see langword="null"/>.</exception>
    public StartsWithExpression(StringSegment value) : this(new StringSegmentLinkedList(value))
    {
        if (!value.HasValue)
        {
            throw new ArgumentNullException(nameof(value));
        }
    }

    /// <summary>
    /// Builds a new <see cref="StartsWithExpression"/> which <see cref="Value"/> is initialized from <paramref name="value"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public StartsWithExpression(ConstantValueExpression value)
    {
        (_lazyEscapedParseableString, Value) = value switch
        {
            null => throw new ArgumentNullException(nameof(value)),
            TextExpression text => (new Lazy<EscapedString>(() => EscapedString.From($"{text.EscapedParseableString}*")), text.Value),
            _ => (new Lazy<EscapedString>(() => EscapedString.From($"{value.EscapedParseableString}*" )), value.Value )
        };
    }

    /// <summary>
    /// Builds a new <see cref="StartsWithExpression"/> that holds the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value associated with the expression</param>
    /// <exception cref="ArgumentOutOfRangeException">if <paramref name="value"/> is <see cref="string.Empty"/>.</exception>
    /// <remarks>
    /// The constructor will take care of escaping all specific characters of <paramref name="value"/>
    /// </remarks>
    public StartsWithExpression(StringSegmentLinkedList value)
    {
        Value = value switch
        {
            { Count: 0 } => throw new ArgumentOutOfRangeException(nameof(value)),
            _ => value
        };

        _lazyEscapedParseableString = new Lazy<EscapedString>(() =>
            {
                string escapedValue = new StringSegmentLinkedList().Append(Value.Replace(chr => FilterTokenizer.SpecialCharacters.Contains(chr), FilterTokenizer.EscapedSpecialCharacters))
                    .ToStringValue();

                return EscapedString.From($"{escapedValue}*");
        });
    }

    /// <summary>
    /// Builds a new <see cref="StartsWithExpression"/> that holds the specified <paramref name="text"/>.
    /// </summary>
    /// <param name="text"></param>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
    public StartsWithExpression(TextExpression text)
        : this(Guard.Against.Null(text).Value)
    {
        _lazyEscapedParseableString = new Lazy<EscapedString>(() => EscapedString.From($"{text.EscapedParseableString}*"));
    }

    ///<inheritdoc/>
    public bool Equals(StartsWithExpression other) => Value.Equals(other?.Value);

    ///<inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as StartsWithExpression);

    ///<inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    ///<inheritdoc/>
    public override EscapedString EscapedParseableString => _lazyEscapedParseableString.Value;

    /// <summary>
    /// Combines the specified <paramref name="left"/> and <paramref name="right"/> <see cref="StartsWithExpression"/>s into a new <see cref="AndExpression"/>.
    /// </summary>
    /// <param name="left">The left operand</param>
    /// <param name="right">The right operand</param>
    /// <returns>a <see cref="AndExpression"/> whose <see cref="BinaryFilterExpression.Left"/> is <paramref name="left"/> and <see cref="BinaryFilterExpression.Right"/> is
    /// <paramref name="right"/></returns>
    /// <exception cref="ArgumentNullException">if either <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.</exception>
    public static AndExpression operator +(StartsWithExpression left, EndsWithExpression right) => new(left, right);

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
    public static OneOfExpression operator +(StartsWithExpression left, StartsWithExpression right)
    {
        StringSegmentLinkedList leftConcatRight = new StringSegmentLinkedList().Append(left.Value).Append(right.Value);

        return new OneOfExpression(new StringValueExpression(leftConcatRight), new StartsWithExpression(leftConcatRight), new AndExpression(left, new ContainsExpression(right.Value)));
    }

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
        StringSegmentLinkedList value = new StringSegmentLinkedList().Append(left.Value).Append(right.Value);
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
    public static AndExpression operator +(StartsWithExpression left, StringValueExpression right) => left + new EndsWithExpression(right.EscapedParseableString);

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
    public static NotExpression operator !(StartsWithExpression expression) => new(expression);
}
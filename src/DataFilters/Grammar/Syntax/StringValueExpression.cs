
using System;
using System.Linq;
using Candoumbe.Types.Strings;
using static DataFilters.Grammar.Parsing.FilterTokenizer;

namespace DataFilters.Grammar.Syntax;

/// <summary>
/// Wraps a string that represents a constant string value
/// </summary>
public class StringValueExpression : ConstantValueExpression, IEquatable<StringValueExpression>
{
    private readonly Lazy<string> _lazyParseableString;
    private readonly Lazy<string> _lazyOriginalString;

    /// <summary>
    /// Builds a new <see cref="StringValueExpression"/> instance that can wrap a string value
    /// </summary>
    /// <remarks>
    /// The <see cref="EscapedParseableString"/> property automatically escapes <see cref="SpecialCharacters"/> from <paramref name="value"/>.
    /// </remarks>
    /// <param name="value">value of the expression.</param>
    /// <exception cref="ArgumentOutOfRangeException">value is empty</exception>
    public StringValueExpression(ReadOnlySpan<char> value) : this(new StringSegmentLinkedList(value))
    {
        // if (!value.HasValue)
        // {
        //     throw new ArgumentNullException(nameof(value));
        // }

        if (value.IsEmpty)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value.ToString(), $"{nameof(value)} cannot be null or empty");
        }
    }

    /// <summary>
    /// Builds a new <see cref="StringValueExpression"/> instance that can wrap a string value
    /// </summary>
    /// <param name="value">value of the expression.</param>
    public StringValueExpression(StringSegmentLinkedList value) : base(value)
    {
        _lazyOriginalString = new Lazy<string>(value.ToStringValue);
        _lazyParseableString = new Lazy<string>(() => Value.Replace(chr => SpecialCharacters.Contains(chr),
                                      EscapedSpecialCharacters).ToStringValue());
    }

    ///<inheritdoc/>
    public override string EscapedParseableString => _lazyParseableString.Value;

    ///<inheritdoc/>
    public override string OriginalString => _lazyOriginalString.Value;

    ///<inheritdoc/>
    public virtual bool Equals(StringValueExpression other) => Equals(_lazyOriginalString.Value, other?._lazyOriginalString.Value);

    ///<inheritdoc/>
    public override bool Equals(object obj) =>
        obj switch
        {
            NumericValueExpression numericValue => _lazyOriginalString.Value.Equals(numericValue.OriginalString),
            TextExpression text => text.EscapedParseableString == OriginalString,
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

    /// <summary>
    /// Checks if <paramref name="left"/> and <paramref name="right"/> values are equal.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are equal, and <see langword="false"/> otherwise.</returns>
    public static bool operator ==(StringValueExpression left, StringValueExpression right)
        => (left is null && right is null) || (left?.Equals(right) ?? false);

    /// <summary>
    /// Checks if <paramref name="left"/> and <paramref name="right"/> values are not equal.
    /// </summary>
    /// <param name="left">left operand</param>
    /// <param name="right">right operand</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is not equal to <paramref name="right"/>, and <see langword="false"/> otherwise.</returns>
    public static bool operator !=(StringValueExpression left, StringValueExpression right)
        => !(left == right);
}
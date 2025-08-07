
using System;
using System.Collections.Generic;

namespace DataFilters.Grammar.Syntax;
/// <summary>
/// Stores a constant value used in a bracket expression
/// </summary>
public sealed class ConstantBracketValue : BracketValue, IEquatable<ConstantBracketValue>
{
    private readonly Lazy<string> _lazyEscapedParseableString;

    /// <summary>
    /// Builds a new <see cref="ConstantBracketValue"/> instance.
    /// </summary>
    /// <param name="value"></param>
    public ConstantBracketValue(string value)
    {
        Value = value;
        _lazyEscapedParseableString = new(() => $"[{Value}]");
    }

    /// <summary>
    /// Constant value of the regex
    /// </summary>
    public string Value { get; }

    ///<inheritdoc/>
    public bool Equals(ConstantBracketValue other) => Value.Equals(other?.Value);

    ///<inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj switch
        {
            RangeBracketValue rangeBracket => rangeBracket.Equals(this),
            _ => Equals(obj as ConstantBracketValue),
        };
    }

    ///<inheritdoc />
    public static bool operator ==(ConstantBracketValue left, ConstantBracketValue right) => EqualityComparer<ConstantBracketValue>.Default.Equals(left, right);

    ///<inheritdoc />
    public static bool operator !=(ConstantBracketValue left, ConstantBracketValue right) => !(left == right);

    ///<inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    ///<inheritdoc />
    public override string EscapedParseableString => _lazyEscapedParseableString.Value;

    ///<inheritdoc/>
    public override string OriginalString => $"[{Value}]";

    ///<inheritdoc/>
    public override double Complexity => 1 + Math.Pow(2, Value.Length);
}
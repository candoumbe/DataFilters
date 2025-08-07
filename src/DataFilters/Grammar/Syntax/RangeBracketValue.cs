
using System;
using System.Collections.Generic;

namespace DataFilters.Grammar.Syntax;
/// <summary>
/// stores a range value used in a bracket expression
/// </summary>
/// <remarks>
/// Builds a new <see cref="RangeBracketValue"/> instance.
/// </remarks>
/// <param name="start"></param>
/// <param name="end"></param>
public sealed class RangeBracketValue(char start, char end) : BracketValue, IEquatable<RangeBracketValue>, IComparable<RangeBracketValue>
{
    /// <summary>
    /// Start of the regex range
    /// </summary>
    public char Start { get; } = start;

    /// <summary>
    /// Ends of the regex range
    /// </summary>
    public char End { get; } = end;

    ///<inheritdoc/>
    public bool Equals(RangeBracketValue other) => (Start, End) == (other?.Start, other?.End);

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            bool equals;
            if (obj is ConstantBracketValue constantBracketValue)
            {
                char[] chars = [.. constantBracketValue.Value];
                char head = chars[0];
                char tail = chars[^1];
                equals = (Start, End).Equals((head, tail));
            }
            else
            {
                equals = Equals(obj as RangeBracketValue);
            }

            return equals;
    }

    ///<inheritdoc />
    public static bool operator ==(RangeBracketValue left, RangeBracketValue right) => EqualityComparer<RangeBracketValue>.Default.Equals(left, right);

    ///<inheritdoc />
    public static bool operator !=(RangeBracketValue left, RangeBracketValue right) => !(left == right);

    ///<inheritdoc />
    public static bool operator <(RangeBracketValue left, RangeBracketValue right) => left.Start < right.Start;

    ///<inheritdoc />
    public static bool operator >(RangeBracketValue left, RangeBracketValue right) => left.Start > right.Start;

    ///<inheritdoc />
    public static bool operator <=(RangeBracketValue left, RangeBracketValue right) => left < right || left == right;

    ///<inheritdoc />
    public static bool operator >=(RangeBracketValue left, RangeBracketValue right) => left > right || left == right;

    ///<inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Start, End);

    ///<inheritdoc />
    public override string EscapedParseableString => $"[{Start}-{End}]";

    ///<inheritdoc />
    public override string OriginalString => $"[{Start}-{End}]";

    ///<inheritdoc />
    public override double Complexity => 1 + Math.Pow(2, End - Start);

    ///<inheritdoc />
    public int CompareTo(RangeBracketValue other) => (Start, End).CompareTo((other.Start, other.End));
}
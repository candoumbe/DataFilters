using DataFilters.ValueObjects;

namespace DataFilters.Grammar.Syntax
{
    using System;
    using System.Collections.Generic;

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
            switch (obj)
            {
                case ConstantBracketValue constantBracketValue:
                    char[] chars = [.. constantBracketValue.Value];
                    char head = chars[0];
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                    char tail = chars[^1];
#else
                    char tail = chars[chars.Length - 1];
#endif
                    equals = (Start, End).Equals((head, tail));
                    break;
                default:
                    equals = Equals(obj as RangeBracketValue);
                    break;
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
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        public override int GetHashCode() => HashCode.Combine(Start, End);
#else
        public override int GetHashCode()
        {
            int hashCode = -1676728671;
            hashCode = (hashCode * -1521134295) + Start.GetHashCode();
            hashCode = (hashCode * -1521134295) + End.GetHashCode();
            return hashCode;
        }

#endif

        ///<inheritdoc />
        public override EscapedString EscapedParseableString => EscapedString.From($"[{Start}-{End}]");

        ///<inheritdoc />
        public override string OriginalString => $"[{Start}-{End}]";

        ///<inheritdoc />
        public override double Complexity => 1 + Math.Pow(2, End - Start);

        ///<inheritdoc />
        public int CompareTo(RangeBracketValue other) => (Start, End).CompareTo((other.Start, other.End));
    }
}
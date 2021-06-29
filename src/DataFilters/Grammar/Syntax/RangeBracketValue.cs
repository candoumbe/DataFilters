namespace DataFilters.Grammar.Syntax
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// stores a range value used in a bracket expression
    /// </summary>
    public sealed class RangeBracketValue : BracketValue, IEquatable<RangeBracketValue>, IComparable<RangeBracketValue>
    {
        /// <summary>
        /// Builds a new <see cref="RangeBracketValue"/> instance.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public RangeBracketValue(char start, char end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Start of the regex range
        /// </summary>
        public char Start { get; }

        /// <summary>
        /// Ends of the regex range
        /// </summary>
        public char End { get; }

        ///<inheritdoc/>
        public bool Equals(RangeBracketValue other) => (Start, End) == (other?.Start, other?.End);

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            bool equals;
            switch (obj)
            {
                case ConstantBracketValue constantBracketValue:
                    char[] chrs = constantBracketValue.Value.ToCharArray();
                    char head = chrs[0];
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                    char tail = chrs[^1];
#else
                    char tail = chrs[chrs.Length - 1];
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
#if NETSTANDARD2_1_OR_GREATER
        public override int GetHashCode() => HashCode.Combine(Start, End);
#else
        public override int GetHashCode()
        {
            int hashCode = -1676728671;
            hashCode = hashCode * -1521134295 + Start.GetHashCode();
            hashCode = hashCode * -1521134295 + End.GetHashCode();
            return hashCode;
        }

#endif

        ///<inheritdoc />
        public override string ToString() => $"[{Start}-{End}]";

        ///<inheritdoc />
        public int CompareTo(RangeBracketValue other) => (Start, End).CompareTo((other.Start, other.End));
    }
}
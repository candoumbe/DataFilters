using System;
using System.Collections.Generic;

namespace DataFilters.Grammar.Syntax
{

    /// <summary>
    /// stores a range value used in a regex expression
    /// </summary>
#if NET5_0_OR_GREATER
    public record RegexRangeValue(char Start, char End): RegexValue;
#else
    public sealed class RegexRangeValue : RegexValue, IEquatable<RegexRangeValue>
    {
        /// <summary>
        /// Builds a new <see cref="RegexRangeValue"/> instance.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public RegexRangeValue(char start, char end)
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
        public bool Equals(RegexRangeValue other) => (Start, End) == (other?.Start, other?.End);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as RegexRangeValue);


        ///<inheritdoc />
        public static bool operator ==(RegexRangeValue left, RegexRangeValue right) => EqualityComparer<RegexRangeValue>.Default.Equals(left, right);

        ///<inheritdoc />
        public static bool operator !=(RegexRangeValue left, RegexRangeValue right) => !(left == right);

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
    }
#endif
}
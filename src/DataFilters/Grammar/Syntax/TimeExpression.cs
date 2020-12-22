using System;

namespace DataFilters.Grammar.Syntax
{
    public class TimeExpression : FilterExpression, IEquatable<TimeExpression>, IBoundaryExpression
    {
        public int Hours { get; }
        public int Minutes { get; }
        public int Seconds { get; }
        public int Milliseconds { get; }

        /// <summary>
        /// Offset with the UTC.
        /// </summary>
        public TimeOffset Offset { get; }

        /// <summary>
        /// Builds a new <see cref="TimeExpression"> instance.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <param name="milliseconds"></param>
        /// <param name="offset"></param>
        public TimeExpression(int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0, TimeOffset offset = null)
        {
            if (hours < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hours), hours, $"{nameof(hours)} must be zero or positive");
            }

            if (minutes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minutes), minutes, $"{nameof(minutes)} must be zero or positive");
            }

            if (seconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), seconds, $"{nameof(seconds)} must be zero or positive");
            }

            if (milliseconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(milliseconds), milliseconds, $"{nameof(milliseconds)} must be zero or positive");
            }

            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Milliseconds = milliseconds;
            Offset = offset;
        }

        public bool Equals(TimeExpression other) => other != null
            && (Hours, Minutes, Seconds, Milliseconds, Offset).Equals((other.Hours, other.Minutes, other.Seconds, other.Milliseconds, other.Offset));

        public override int GetHashCode() => (Hours, Minutes, Seconds, Milliseconds, Offset).GetHashCode();

        public override bool Equals(object obj) => Equals(obj as TimeExpression);

        public override string ToString() => $"{{{Hours:D2}:{Minutes:D2}:{Seconds:D2}{Offset}}}";
    }
}

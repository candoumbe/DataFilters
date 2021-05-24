
using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// A <see cref="FilterExpression"/> that only consists of time part.
    /// </summary>
    public sealed class TimeExpression : FilterExpression, IEquatable<TimeExpression>, IBoundaryExpression
    {
        /// <summary>
        /// Hours part of the expression
        /// </summary>
        public int Hours { get; }

        /// <summary>
        /// Minutes of the expression
        /// </summary>
        public int Minutes { get; }

        /// <summary>
        /// Seconds of the expression
        /// </summary>
        public int Seconds { get; }

        /// <summary>
        /// Milliseconds of the expression
        /// </summary>
        public int Milliseconds { get; }

        /// <summary>
        /// Offset with the UTC.
        /// </summary>
        public TimeOffset Offset { get; }

        /// <summary>
        /// Builds a new <see cref="TimeExpression"/> instance.
        /// </summary>
        /// <remarks>
        /// A time expression can optionally specify an <paramref name="offset"/> with the UTC time
        /// </remarks>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <param name="milliseconds"></param>
        /// <param name="offset"></param>
        /// <exception cref="ArgumentOutOfRangeException">either <paramref name="hours"/>, <paramref name="minutes"/>, <paramref name="seconds"/>, <paramref name="milliseconds"/> </exception>
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

        ///<inheritdoc/>
        public bool Equals(TimeExpression other) => other != null
            && (Hours, Minutes, Seconds, Milliseconds, Offset).Equals((other.Hours, other.Minutes, other.Seconds, other.Milliseconds, other.Offset));

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as TimeExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => (Hours, Minutes, Seconds, Milliseconds, Offset).GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => $"{{{Hours:D2}:{Minutes:D2}:{Seconds:D2}{Offset}}}";
    }
}

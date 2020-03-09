using System;

namespace DataFilters.Grammar.Syntax
{
    public class TimeExpression : FilterExpression, IEquatable<TimeExpression>, IBoundaryExpression
    {
        public int Hours { get; }
        public int Minutes { get; }
        public int Seconds { get; }
        public int Milliseconds { get; }

        public TimeExpression(int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
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
        }

        public bool Equals(TimeExpression other) => other != null
            && (Hours, Minutes, Seconds, Milliseconds).Equals((other.Hours, other.Minutes, other.Seconds, other.Milliseconds));

        public override int GetHashCode() => (Hours, Minutes, Seconds, Milliseconds).GetHashCode();

        public override bool Equals(object obj) => Equals(obj as TimeExpression);

        public override string ToString() => this.Jsonify();
    }
}

namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// A <see cref="FilterExpression"/> that only consists of time part.
    /// </summary>
    public sealed class TimeExpression : FilterExpression, IEquatable<TimeExpression>, IBoundaryExpression, IFormattable
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
        /// Builds a new <see cref="TimeExpression"/> instance.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <param name="milliseconds"></param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// either <paramref name="hours"/>, <paramref name="minutes"/>, <paramref name="seconds"/>, <paramref name="milliseconds"/> is &lt; 0.
        /// </exception>
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

        ///<inheritdoc/>
        public bool Equals(TimeExpression other) => other is not null
            && ((Hours, Minutes, Seconds, Milliseconds) == (other.Hours, other.Minutes, other.Seconds, other.Milliseconds));

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as TimeExpression);

        ///<inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other) => other switch
        {
            TimeExpression time => Equals(time),
            DateTimeExpression { Date: null, Time: var time, Offset: null } => Equals(time),
            _ => Equals((other as ISimplifiable)?.Simplify() ?? other)
        };

        ///<inheritdoc/>
        public override int GetHashCode() => (Hours, Minutes, Seconds, Milliseconds).GetHashCode();

        ///<inheritdoc/>
        public override string EscapedParseableString => $"{Hours:D2}:{Minutes:D2}:{Seconds:D2}{(Milliseconds > 0 ? $".{Milliseconds}" : string.Empty)}";

        ///<inheritdoc/>
        public void Deconstruct(out int hours, out int minutes, out int seconds, out int milliseconds)
        {
            hours = Hours;
            minutes = Minutes;
            seconds = Seconds;
            milliseconds = Milliseconds;
        }

        /// <inheritdoc />
        public static bool operator ==(TimeExpression left, TimeExpression right) => left switch
        {
            null => right is null,
            _ => left.Equals(right)
        };

        /// <inheritdoc />
        public static bool operator !=(TimeExpression left, TimeExpression right) => !(left == right);
    }
}

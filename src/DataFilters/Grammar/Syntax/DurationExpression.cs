using System;
using System.Text;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// A duration expression (<see cref="https://en.wikipedia.org/wiki/ISO_8601#Durations"/>)
    /// </summary>
    public class DurationExpression : FilterExpression, IEquatable<DurationExpression>
    {
        public int Years { get; }
        public int Months { get; }
        public int Days { get; }
        public int Hours { get; }
        public int Minutes { get; }
        public int Seconds { get; }
        public int Weeks { get; }

        /// <summary>
        /// Builds a new <see cref="DurationExpression"/> instance
        /// </summary>
        /// <param name="years"></param>
        /// <param name="months"></param>
        /// <param name="weeks"></param>
        /// <param name="days"></param>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        public DurationExpression(int years = 0, int months = 0, int weeks = 0, int days = 0, int hours = 0, int minutes = 0, int seconds = 0)
        {
            if (years < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(years), years, $"{nameof(years)} cannot be negative");
            }

            if (months < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(months), months, $"{nameof(months)} cannot be negative");
            }

            if (weeks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weeks), weeks, $"{nameof(weeks)} cannot be negative");
            }

            if (days < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(days), days, $"{nameof(days)} cannot be negative");
            }

            if (hours < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hours), hours, $"{nameof(hours)} cannot be negative");
            }

            if (minutes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minutes), minutes, $"{nameof(minutes)} cannot be negative");
            }

            if (seconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), seconds, $"{nameof(seconds)} cannot be negative");
            }

            Years = years;
            Months = months;
            Weeks = weeks;
            Days = days;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
        }

        public override bool Equals(object obj) => Equals(obj as DurationExpression);

        public bool Equals(DurationExpression other) => (Years, Months, Weeks, Days, Hours, Minutes, Seconds) == (other?.Years, other?.Months, other?.Weeks, other?.Days, other?.Hours, other?.Minutes, other?.Seconds);

        public override int GetHashCode() => (Years, Months, Weeks, Days, Hours, Minutes, Seconds).GetHashCode();

        public override bool IsEquivalentTo(FilterExpression other)
        {
            static DateTime ConvertToDateTime(DurationExpression duration)
            {
                return DateTime.MinValue.AddYears(duration.Years)
                                        .AddMonths(duration.Months)
                                        .AddDays((duration.Weeks * 7) + duration.Days)
                                        .AddHours(duration.Hours)
                                        .AddMinutes(duration.Minutes)
                                        .AddSeconds(duration.Seconds);
            }

            bool equivalent = false;

            if (other is DurationExpression otherDuration)
            {
                equivalent = Equals(otherDuration);
                if (!equivalent)
                {
                    DateTime otherDateTime = ConvertToDateTime(otherDuration);
                    DateTime current = ConvertToDateTime(this);

                    equivalent = current - otherDateTime == TimeSpan.Zero;
                }
            }

            return equivalent;
        }

        public override string ToString() => this.Jsonify();
    }
}
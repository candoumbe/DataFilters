namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// <see cref="TimeOffset"/> records the offset between a time and the UTC time
    /// </summary>
#if NETSTANDARD2_1 || NET5_0
    public record TimeOffset
    {
        /// <summary>
        /// Gets the number of hours of offset with the UTC time
        /// </summary>
        public int Hours { get; }

        /// <summary>
        /// Get the number of minutes of offset with the UTC time
        /// </summary>
        public int Minutes { get; }

        /// <summary>
        /// The Zero time offset
        /// </summary>
        public static TimeOffset Zero => new();

        /// <summary>
        /// Builds a new <see cref="TimeOffset"/> instance
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        public TimeOffset(int hours = 0, int minutes = 0)
        {
            Hours = hours;
            Minutes = minutes;
        }

        ///<inheritdoc/>
        public override string ToString() => (Hours, Minutes) switch
        {
            (0, 0 ) => "Z",
            ( > 0, >= 0) => $"+{Hours:D2}:{Minutes:D2}",
            ( <= 0, < 0) => $"-{Math.Abs(Hours):D2}:{Math.Abs(Minutes):D2}",
            _ => $"{Hours:D2}:{Minutes:D2}"
        };
    }
#elif NETSTANDARD1_3 || NETSTANDARD2_0
    public sealed class TimeOffset : IEquatable<TimeOffset>
    {
        /// <summary>
        /// Gets the number of hours of offset with the UTC time
        /// </summary>
        public int Hours { get; }

        /// <summary>
        /// Gets the number of minutes of offset with the UTC time
        /// </summary>
        public int Minutes { get; }

        /// <summary>
        /// The Zero time offset
        /// </summary>
        public static TimeOffset Zero => new();

        /// <summary>
        /// Builds a new <see cref="TimeOffset"/> instance
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        public TimeOffset(int hours = 0, int minutes = 0)
        {
            Hours = hours;
            Minutes = minutes;
        }

        ///<inheritdoc/>
        public override string ToString() => (Hours, Minutes) switch
        {
            (0, 0) => "Z",
            ( > 0, >= 0) => $"+{Hours:D2}:{Minutes:D2}",
            ( <= 0, < 0) => $"-{Math.Abs(Hours):D2}:{Math.Abs(Minutes):D2}",
            _ => $"{Hours:D2}:{Minutes:D2}"
        };

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as TimeOffset);

        ///<inheritdoc/>
        public bool Equals(TimeOffset other) => (Hours, Minutes).Equals((other?.Hours, other?.Minutes));

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -793696894;
            hashCode = (hashCode * -1521134295) + Hours.GetHashCode();
            hashCode = (hashCode * -1521134295) + Minutes.GetHashCode();
            return hashCode;
        }
    }
#else
#error Not supported framework
#endif
}

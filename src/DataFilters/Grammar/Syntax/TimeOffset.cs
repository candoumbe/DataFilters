using System;

namespace DataFilters.Grammar.Syntax
{

#if NETSTANDARD2_1 || NET5_0
    public record TimeOffset
    {
        public int Hours { get; }
        public int Minutes { get; }

        public TimeOffset(int hours = 0, int minutes = 0)
        {
            Hours = hours;
            Minutes = minutes;
        }

        public override string ToString() => (Hours, Minutes) switch
        {
            (0, 0 ) => "Z",
            ( > 0, >= 0) => $"+{Hours:D2}:{Minutes:D2}",
            ( <= 0, < 0) => $"-{Math.Abs(Hours):D2}:{Math.Abs(Minutes):D2}",
            _ => $"{Hours:D2}:{Minutes:D2}"
        };
    } 
#elif NETSTANDARD1_3 || NETSTANDARD2_0
    public class TimeOffset : IEquatable<TimeOffset>
    {
        public int Hours { get; }

        public int Minutes { get; }

        public TimeOffset(int hours = 0, int minutes = 0)
        {
            Hours = hours;
            Minutes = minutes;
        }

        public override string ToString() => (Hours, Minutes) switch
        {
            (0, 0) => "Z",
            ( > 0, >= 0) => $"+{Hours:D2}:{Minutes:D2}",
            ( <= 0, < 0) => $"-{Math.Abs(Hours):D2}:{Math.Abs(Minutes):D2}",
            _ => $"{Hours:D2}:{Minutes:D2}"
        };

        public override bool Equals(object obj) => Equals(obj as TimeOffset);

        public bool Equals(TimeOffset other) => (Hours, Minutes).Equals((other?.Hours, other.Minutes));

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

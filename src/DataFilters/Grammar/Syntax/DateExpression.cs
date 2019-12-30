using System;

namespace DataFilters.Grammar.Syntax
{
    public class DateExpression : FilterExpression, IEquatable<DateExpression>, IBoundaryExpression
    {
        public int Year { get; }
        public int Month { get; }
        public int Day { get; }

        public DateExpression(int year = 1, int month = 1, int day = 1)
        {
            if (year < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(year));
            }

            if (month < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(month));
            }

            if (day < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(day));
            }

            Year = year;
            Month = month;
            Day = day;
        }

        public bool Equals(DateExpression other) => other != null
            && (Year, Month, Day).Equals((other.Year, other.Month, other.Day));

        public override bool Equals(object obj) => Equals(obj as DateExpression);

        public override int GetHashCode() => (Year, Month, Day).GetHashCode();

        public override string ToString() => this.Jsonify();
    }
}

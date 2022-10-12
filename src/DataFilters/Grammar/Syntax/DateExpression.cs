namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// A <see cref="FilterExpression"/> that holds a date.
    /// </summary>
    public sealed class DateExpression : FilterExpression, IEquatable<DateExpression>, IBoundaryExpression
    {
        /// <summary>
        /// Year part of the date
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// Month part of the date
        /// </summary>
        public int Month { get; }

        /// <summary>
        /// Day part of the date
        /// </summary>
        public int Day { get; }

        /// <summary>
        /// Builds a new <see cref="DateExpression"/> instance.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// either <paramref name="year"/> / <paramref name="month"/> or <paramref name="day"/> is less than <c>1</c>.
        /// </exception>
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

        /// <inheritdoc />
        public bool Equals(DateExpression other) => (Year, Month, Day) == (other?.Year, other?.Month, other?.Day);

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as DateExpression);

        /// <inheritdoc />
        public override bool IsEquivalentTo(FilterExpression other) => other switch
        {
            DateExpression date => Equals(date),
            DateTimeExpression { Date : var date, Time : null, Offset: null } => Equals(date),
            _ => Equals((other as ISimplifiable)?.Simplify() ?? other)
        };

        /// <inheritdoc />
        public override int GetHashCode() => (Year, Month, Day).GetHashCode();

        /// <inheritdoc />
        public override string EscapedParseableString => $"{Year:D4}-{Month:D2}-{Day:D2}";

        /// <inheritdoc />
        public static bool operator ==(DateExpression left, DateExpression right) => left switch
        {
            null => right is null,
            _ => left.Equals(right)
        };

        /// <inheritdoc />
        public static bool operator !=(DateExpression left, DateExpression right) => ! (left == right);
    }
}

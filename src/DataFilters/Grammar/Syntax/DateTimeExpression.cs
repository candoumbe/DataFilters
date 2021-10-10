namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// A <see cref="FilterExpression"/> implementation that can holds a datetime value
    /// </summary>
    public sealed class DateTimeExpression : FilterExpression, IEquatable<DateTimeExpression>, IBoundaryExpression
    {
        private readonly Lazy<string> _lazyEscapedParseableString;

        /// <summary>
        /// Date part of the expression
        /// </summary>
        public DateExpression Date { get; }

        /// <summary>
        /// Time part of the expression
        /// </summary>
        public TimeExpression Time { get; }

        /// <summary>
        /// Offset with the UTC.
        /// </summary>
        public OffsetExpression Offset { get; }

        /// <summary>
        /// Definies the kind of the current <see cref="DateTimeExpression"/> instance.
        /// </summary>
        public DateTimeExpressionKind Kind { get; }

        /// <summary>
        /// Builds a new <see cref="DateTimeExpression"/> instance where only <see cref="Date"/> part is set
        /// </summary>
        /// <param name="date"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="date"/> is <c>null</c>.</exception>
        public DateTimeExpression(DateExpression date) : this(date, null)
        {
        }

        /// <summary>
        /// Builds a new <see cref="DateTimeExpression"/> where only the <see cref="Time"/> part is specified
        /// </summary>
        /// <param name="time"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="time"/> is <c>null</c>.</exception>
        public DateTimeExpression(TimeExpression time) : this(null, time) { }

        /// <summary>
        /// Builds a new <see cref="DateTimeExpression"/> with both <paramref name="date"/> and <paramref name="time"/>
        /// specified.
        /// </summary>
        /// <param name="date">date part of the expression</param>
        /// <param name="time">time part of the expression</param>
        /// <exception cref="ArgumentException">if both <paramref name="date"/> and <paramref name="time"/> are <c>null</c>.</exception>
        public DateTimeExpression(DateExpression date, TimeExpression time) : this(date, time, null)
        {
        }

        /// <summary>
        /// Builds a new <see cref="DateTimeExpression"/> with both <paramref name="date"/> and <paramref name="time"/>
        /// specified.
        /// </summary>
        /// <param name="date">date part of the expression</param>
        /// <param name="time">time part of the expression</param>
        /// <param name="offset">offset with the UTC time</param>
        /// <exception cref="ArgumentException">if both <paramref name="date"/> and <paramref name="time"/> are <c>null</c>.</exception>
        public DateTimeExpression(DateExpression date, TimeExpression time, OffsetExpression offset)
        {
            if (date is null && time is null)
            {
                throw new ArgumentException($"Both {nameof(date)} and {nameof(time)} cannot be null and ");
            }

            Date = date;
            Time = time;
            Offset = offset;
            Kind = offset is not null
                ? DateTimeExpressionKind.Utc
                : DateTimeExpressionKind.Unspecified;

            _lazyEscapedParseableString = new(() => (date, time, offset) switch
           {
               ({ }, { }, { }) => $"{date.EscapedParseableString}T{time.EscapedParseableString}{offset.EscapedParseableString}",
               ({ }, { }, null) => $"{date.EscapedParseableString}T{time.EscapedParseableString}",
               (null, { }, _) => $"T{time.EscapedParseableString}",
               _ => date.EscapedParseableString
           });
        }

        /// <summary>
        /// Builds a new <see cref="DateTimeExpression"/> with both <see cref="Date"/> and <see cref="Time"/> values
        /// extracted from .
        /// </summary>
        /// <param name="localDateTime"><see cref="DateTime"/> value to use as source of the current instance</param>
        public DateTimeExpression(DateTime localDateTime) : this(new DateExpression(localDateTime.Year, localDateTime.Month, localDateTime.Day), new TimeExpression(localDateTime.Hour, localDateTime.Minute, localDateTime.Second))
        {
        }

        /// <inheritdoc/>
        public void Deconstruct(out DateExpression date, out TimeExpression time, out OffsetExpression offset, out DateTimeExpressionKind kind)
        {
            date = Date;
            time = Time;
            offset = Offset;
            kind = Offset is null
                ? DateTimeExpressionKind.Unspecified
                : DateTimeExpressionKind.Utc;
        }

        /// <inheritdoc/>
        public bool Equals(DateTimeExpression other) => (Date, Time, Offset) == (other?.Date, other?.Time, other?.Offset);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj switch
        {
            DateTimeExpression dateTime => Equals(dateTime),
            DateExpression date => Time is null && Offset is null && Date.Equals(date),
            TimeExpression time => Date is null && Offset is null && Time.Equals(time),
            _ => false
        };

        /// <inheritdoc/>
        public override int GetHashCode() => (Date, Time, Offset).GetHashCode();

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyEscapedParseableString.Value;

        /// <inheritdoc />
        public static bool operator ==(DateTimeExpression left, DateTimeExpression right) => left switch
        {
            null => right is null,
            _ => left.Equals(right)
        };

        /// <inheritdoc />
        public static bool operator !=(DateTimeExpression left, DateTimeExpression right) => !(left == right);
    }
}

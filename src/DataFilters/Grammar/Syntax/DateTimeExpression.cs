using System;

namespace DataFilters.Grammar.Syntax
{
    public class DateTimeExpression : FilterExpression, IEquatable<DateTimeExpression>, IBoundaryExpression
    {
        /// <summary>
        /// Date part of the expression
        /// </summary>
        public DateExpression Date { get; }

        /// <summary>
        /// Time part of the expression
        /// </summary>
        public TimeExpression Time { get; }

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
        /// <exception cref="ArgumentNullException">if both <paramref name="date"/> and <paramref name="time"/> are <c>null</c>.</exception>
        public DateTimeExpression(DateExpression date, TimeExpression time)
        {
            if (date is null && time is null)
            {
                throw new ArgumentNullException($"Both {nameof(date)} and {nameof(time)} cannot be null");
            }
            Date = date;
            Time = time;
            Kind = time?.Offset is not null
                ? DateTimeExpressionKind.Utc
                : DateTimeExpressionKind.Unspecified;
        }

        /// <summary>
        /// Builds a new <see cref="DateTimeExpression"/> with both <see cref="Date"/> and <see cref="Time"/> values
        /// extracted from .
        /// </summary>
        /// <param name="utc"><see cref="DateTime"/> value to use as source of the current instance</param>
        public DateTimeExpression(DateTime utc)
            : this(new DateExpression(utc.Year, utc.Month, utc.Day), new TimeExpression(utc.Hour, utc.Minute, utc.Second))
        {
        }

        /// <inheritdoc/>
        public bool Equals(DateTimeExpression other) => (Date, Time).Equals((other?.Date, other?.Time));

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as DateTimeExpression);

        /// <inheritdoc/>
        public override int GetHashCode() => (Date, Time).GetHashCode();

        /// <inheritdoc/>
        public void Deconstruct(out DateExpression date, out TimeExpression time)
        {
            date = Date;
            time = Time;
        }

        public override string ToString() => this.Jsonify();
    }
}

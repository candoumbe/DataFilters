using System;
using static Newtonsoft.Json.JsonConvert;

namespace DataFilters.Grammar.Syntax
{
    public class DateTimeExpression : FilterExpression, IEquatable<DateTimeExpression>, IBoundaryExpression
    {
        public DateExpression Date { get; }
        public TimeExpression Time { get; }

        /// <summary>
        /// Builds a new <see cref="DateTimeExpression"/> where the <see cref="Date"/> is not set
        /// </summary>
        /// <param name="date">DateExpression</param>
        public DateTimeExpression(DateExpression date) : this(date, null)
        {

        }

        /// <summary>
        /// Builds a new <see cref="DateTimeExpression"/> where only the <see cref="Time"/> is specified
        /// </summary>
        /// <param name="time"></param>
        public DateTimeExpression(TimeExpression time): this(null, time) { }

        public DateTimeExpression(DateExpression date, TimeExpression time)
        {
            Date = date;
            Time = time;
        }

        public DateTimeExpression(DateTime utc) 
            : this(new DateExpression(utc.Year, utc.Month, utc.Day), new TimeExpression(utc.Hour, utc.Minute, utc.Second))
        {
        }

        public bool Equals(DateTimeExpression other) => (Date, Time).Equals( (other?.Date, other?.Time));

        public override bool Equals(object obj) => Equals(obj as DateTimeExpression);

        public override int GetHashCode() => (Date, Time).GetHashCode();

        public void Deconstruct(out DateExpression date, out TimeExpression time)
        {
            date = Date;
            time = Time;
        }

        public override string ToString() => SerializeObject(new { Date, Time});
    }
}

using DataFilters.Grammar.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// An expression that holds an interval between <see cref="Min"/> and <see cref="Max"/> values
    /// </summary>
    public class RangeExpression : FilterExpression, IEquatable<RangeExpression>
    {
        /// <summary>
        /// Lower bound of the current instance
        /// </summary>
        public IBoundaryExpression Min { get; }

        /// <summary>
        /// Upper bound of the current instance
        /// </summary>
        public IBoundaryExpression Max { get; }

        /// <summary>
        /// Builds a new <see cref="RangeExpression"/> instance
        /// </summary>
        /// <param name="min">Lower bound of the interval</param>
        /// <param name="max">Upper bound of the interval</param>
        /// <exception cref="ArgumentNullException">if both <paramref name="min"/> and <paramref name="max"/> are <c>null</c>.</exception>
        /// <exception cref="BoundariesTypeMismatchException">if <paramref name="min"/> and <paramref name="max"/> types are not "compatible".</exception>
        /// <exception cref="IncorrectBoundaryException">if 
        /// <list type="bullet">
        ///     <item>both<paramref name="min"/> and <paramref name="max"/> types are <see cref="AsteriskExpression"/>.</item>
        ///     <item>both<paramref name="min"/> is <see cref="AsteriskExpression"/> and <paramref name="max"/> <c>null</c>.</item>
        /// </list>
        /// </exception>
        public RangeExpression(IBoundaryExpression min = null, IBoundaryExpression max = null)
        {
            switch (min)
            {
                case AsteriskExpression _ when max is AsteriskExpression:
                    throw new IncorrectBoundaryException(nameof(max), $"{nameof(min)} and {nameof(max)} cannot be both {nameof(AsteriskExpression)} instance");
                case AsteriskExpression _ when max is null:
                    throw new IncorrectBoundaryException(nameof(max), $"{nameof(max)} cannot be null when {nameof(min)} is {nameof(AsteriskExpression)} instance");
                case DateExpression _ when !(max is DateExpression || max is TimeExpression || max is DateTimeExpression || max is null):
                    throw new BoundariesTypeMismatchException($"{nameof(min)}[{min.GetType()}] and {nameof(max)}[{max.GetType()}] types are not compatible", nameof(max));
                case ConstantExpression _ when !(max is AsteriskExpression || max is ConstantExpression || max is null):
                    throw new BoundariesTypeMismatchException($"{nameof(min)}[{min.GetType()}] and {nameof(max)}[{max.GetType()}] types are not compatible", nameof(max));
                case null when max is null:
                    throw new ArgumentNullException($"{nameof(min)} or {nameof(max)} must not be null.");
                case TimeExpression _ when !(max is TimeExpression || max is null):
                    throw new BoundariesTypeMismatchException(nameof(max), $"{nameof(max)} must be a {nameof(TimeExpression)}");
            }
          Min = min switch{
                DateTimeExpression { Date: var date, Time: var time } => (date, time) switch
                {
                    (null, { }) => time,
                    ({ }, null) => date,
                    _ => new DateTimeExpression(date, time)
                },
                null => null,
                _ => min
            };
            Max = max switch
            {
                DateTimeExpression { Date: var date, Time: var time } => (date, time) switch
                {
                    (null, { }) => time,
                    ({ }, null)  => date,
                    _ => new DateTimeExpression(date, time)
                },
                null => null,
                TimeExpression time when min is DateTimeExpression minDateTime => new DateTimeExpression(minDateTime.Date, time),
                _ => max
            };
        }

        public bool Equals(RangeExpression other) => (Min, Max).Equals((other?.Min, other?.Max));

        public override bool Equals(object obj) => Equals(obj as RangeExpression);

        public override int GetHashCode() => (Min, Max).GetHashCode();

        public override string ToString() => this.Jsonify();
    }
}
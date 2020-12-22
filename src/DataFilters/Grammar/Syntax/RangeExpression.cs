using DataFilters.Grammar.Exceptions;
using System;
using System.Runtime.Serialization;

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
        public BoundaryExpression Min { get; }

        /// <summary>
        /// Upper bound of the current instance
        /// </summary>
        public BoundaryExpression Max { get; }

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
        public RangeExpression(BoundaryExpression min = null, BoundaryExpression max = null)
        {
            if (min?.Expression is AsteriskExpression && max?.Expression is AsteriskExpression expression)
            {
                throw new IncorrectBoundaryException(nameof(max), $"{nameof(min)} and {nameof(max)} cannot be both {nameof(AsteriskExpression)} instance");
            }

            if (min?.Expression is AsteriskExpression && max is null)
            {
                throw new IncorrectBoundaryException(nameof(max), $"{nameof(max)} cannot be null when {nameof(min)} is {nameof(AsteriskExpression)} instance");
            }

            if (min?.Expression is DateExpression && !(max is null || max.Expression is DateExpression || max.Expression is TimeExpression || max.Expression is DateTimeExpression))
            {
                throw new BoundariesTypeMismatchException($"{nameof(min)}[{min.GetType()}] and {nameof(max)}[{max.GetType()}] types are not compatible", nameof(max));
            }

            if (min?.Expression is ConstantValueExpression && !(max is null || max.Expression is ConstantValueExpression || max.Expression is AsteriskExpression))
            {
                throw new BoundariesTypeMismatchException($"{nameof(min)}[{min.GetType()}] and {nameof(max)}[{max.GetType()}] types are not compatible", nameof(max));
            }

            if (min is null && max is null)
            {
                throw new ArgumentNullException($"{nameof(min)} or {nameof(max)} must not be null.");
            }

            if (min?.Expression is TimeExpression && !(max is null || max.Expression is TimeExpression))
            {
                throw new BoundariesTypeMismatchException(nameof(max), $"{nameof(max)} must be a {nameof(TimeExpression)}");
            }

            Min = min?.Expression switch
            {
                DateTimeExpression { Date: var date, Time: var time } => (date, time) switch
                {
                    (null, { }) => new BoundaryExpression(time, included: min.Included),
                    ({ }, null) => new BoundaryExpression(date, included: min.Included),
                    _ => new BoundaryExpression(new DateTimeExpression(date, time), included: min.Included)
                },
                null => null,
                AsteriskExpression _ => null,
                _ => min
            };
            Max = max?.Expression switch
            {
                DateTimeExpression { Date: var date, Time: var time } => (date, time) switch
                {
                    (null, { }) => new BoundaryExpression(time, included: max.Included),
                    ({ }, null) => new BoundaryExpression(date, included: max.Included),
                    _ => new BoundaryExpression(new DateTimeExpression(date, time), included: max.Included)
                },
                null => null,
                AsteriskExpression _ => null,
                TimeExpression time when min?.Expression is DateTimeExpression minDateTime => new BoundaryExpression(new DateTimeExpression(minDateTime.Date, time), included: max.Included),
                _ => max
            };
        }

        public bool Equals(RangeExpression other)
        {
            bool equals = false;

            if (other != null)
            {
                if (Min is null)
                {
                    if (other.Min is null)
                    {
                        equals = Max is null
                            ? other.Max is null
                            : Max.Equals(other.Max);
                    }
                }
                else if (Min.Equals(other.Min))
                {
                    equals = Max is null
                        ? other.Max is null
                        : Max.Equals(other.Max);
                }
            }

            return equals;
        }

        public override bool Equals(object obj) => Equals(obj as RangeExpression);

        public override int GetHashCode() => (Min, Max).GetHashCode();

        public override string ToString() => this.Jsonify();
    }
}
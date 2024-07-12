namespace DataFilters.Grammar.Syntax
{
    using System;
    using Exceptions;

    /// <summary>
    /// A <see cref="FilterExpression"/> that holds an interval between <see cref="Min"/> and <see cref="Max"/> values.
    /// </summary>
    public sealed class IntervalExpression : FilterExpression, IEquatable<IntervalExpression>, ISimplifiable
    {
        /// <summary>
        /// Lower bound of the current instance
        /// </summary>
        public BoundaryExpression Min { get; }

        /// <summary>
        /// Upper bound of the current instance
        /// </summary>
        public BoundaryExpression Max { get; }

        private readonly Lazy<string> _lazyToString;
        private readonly Lazy<string> _lazyParseableString;

        /// <summary>
        /// Builds a new <see cref="IntervalExpression"/> instance
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
        /// <remarks>
        ///     Either <paramref name="min"/> or <paramref name="max"/> can be null to indicate and unbounded lower (respectivelu upper) bound.
        /// </remarks>
        public IntervalExpression(BoundaryExpression min = null, BoundaryExpression max = null)
        {
            switch (min?.Expression)
            {
                case AsteriskExpression when max?.Expression is AsteriskExpression expression:
                    throw new IncorrectBoundaryException($"{nameof(min)} and {nameof(max)} cannot be both {nameof(AsteriskExpression)}");
                case AsteriskExpression when max is null:
                    throw new IncorrectBoundaryException($"{nameof(max)} cannot be null when {nameof(min)} is {nameof(AsteriskExpression)}");
                case DateExpression when max is not null && !(max.Expression is AsteriskExpression || max.Expression is DateExpression || max.Expression is TimeExpression || max.Expression is DateTimeExpression):
                    throw new BoundariesTypeMismatchException($"{nameof(min)}[{min?.Expression?.GetType()}] and {nameof(max)}[{max?.Expression?.GetType()}] types are not compatible", nameof(max));
                case ConstantValueExpression when !(max is null || max.Expression is ConstantValueExpression || max.Expression is AsteriskExpression):
                    throw new BoundariesTypeMismatchException($"{nameof(min)}[{min?.Expression?.GetType()}] and {nameof(max)}[{max?.Expression?.GetType()}] types are not compatible", nameof(max));
            }

            if (min is null && max is null)
            {
                throw new IncorrectBoundaryException($"{nameof(min)} or {nameof(max)} must not be null.");
            }

            if (min?.Expression is TimeExpression && max is not null && max.Expression is not TimeExpression && max.Expression is not AsteriskExpression)
            {
                throw new BoundariesTypeMismatchException($"{nameof(max)} must be either {nameof(TimeExpression)} or {nameof(AsteriskExpression)} when min is {nameof(TimeExpression)}", nameof(max));
            }

            Min = min?.Expression switch
            {
                AsteriskExpression or null => null,
                DateTimeExpression { Date: var date, Time: var time, Offset: var offset } => (date, time, offset) switch
                {
                    (null, not null, _) => new BoundaryExpression(time, included: min.Included),
                    (not null, null, _) => new BoundaryExpression(date, included: min.Included),
                    _ => new BoundaryExpression(new DateTimeExpression(date, time, offset), included: min.Included)
                },
                _ => min
            };

            Max = max?.Expression switch
            {
                AsteriskExpression or null => null,
                DateTimeExpression { Date: var date, Time: var time, Offset: var offset } => (date, time, offset) switch
                {
                    (null, not null, _) => new BoundaryExpression(time, included: max.Included),
                    (not null, null, _) => new BoundaryExpression(date, included: max.Included),
                    _ => new BoundaryExpression(new DateTimeExpression(date, time, offset), included: max.Included)
                },
                TimeExpression time when Min?.Expression is DateTimeExpression dateTime => new BoundaryExpression(new DateTimeExpression(dateTime.Date, time, dateTime.Offset), max.Included),
                TimeExpression time => new BoundaryExpression(time, included: max.Included),
                _ => max
            };

            _lazyParseableString = new Lazy<string>(() => $"{GetMinBracket(Min?.Included)}{Min?.Expression?.EscapedParseableString ?? "*"} TO {Max?.Expression?.EscapedParseableString ?? "*"}{GetMaxBracket(Max?.Included)}");
            _lazyToString = new Lazy<string>(() => new
            {
                Min = new
                {
                    Min?.Included,
                    Min?.Expression?.EscapedParseableString,
                    Type = Min?.Expression?.GetType().Name
                },
                Max = new
                {
                    Max?.Included,
                    Max?.Expression?.EscapedParseableString,
                    Type = Max?.Expression?.GetType().Name
                },
                EscapedParseableString
            }
#if NETSTANDARD1_3
        .Jsonify(new Newtonsoft.Json.JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented })
#elif NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
        .Jsonify(new() { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull })
#endif
                )
        ;

            static string GetMinBracket(bool? included) => true.Equals(included) ? "[" : "]";
            static string GetMaxBracket(bool? included) => true.Equals(included) ? "]" : "[";
        }

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as IntervalExpression);

        ///<inheritdoc/>
        public bool Equals(IntervalExpression other)
        {
            bool equals = false;

            if (other is not null)
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

        ///<inheritdoc/>
        public override int GetHashCode() => (Min, Max).GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => _lazyToString.Value;

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyParseableString.Value;

        ///<inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other)
        {
            bool equivalent = false;
            if (other is not null)
            {
                if (ReferenceEquals(this, other))
                {
                    equivalent = true;
                }
                else if (((other as ISimplifiable)?.Simplify() ?? other) is IntervalExpression range)
                {
                    equivalent = Equals(range);
                }
                else if (Min?.Included == true && Max?.Included == true && Min.Equals(Max))
                {
                    equivalent = other.Equals(Min.Expression.As(other.GetType()));
                }
            }

            return equivalent;
        }

        /// <summary>
        /// Deconstruction method
        /// </summary>
        /// <param name="min">lower bound of the interval</param>
        /// <param name="max">Upper bound of the interval</param>
        public void Deconstruct(out BoundaryExpression min, out BoundaryExpression max)
        {
            min = Min;
            max = Max;
        }

        /// <inheritdoc/>
        public override double Complexity => (Min?.Expression?.Complexity ?? 0) + (Max?.Expression?.Complexity ?? 0);

        /// <inheritdoc/>
        public FilterExpression Simplify()
        {
            FilterExpression simplified = this;

            if (Min is not null && Max is not null && Min.Included && Max.Included && Min.Equals(Max))
            {
                simplified = ((FilterExpression)Min.Expression).Complexity < ((FilterExpression)Max.Expression).Complexity
                    ? (FilterExpression)Min.Expression
                    : (FilterExpression)Max.Expression;
            }

            return simplified;
        }
    }
}
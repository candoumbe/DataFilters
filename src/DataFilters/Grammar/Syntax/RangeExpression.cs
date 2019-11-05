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
        public ConstantExpression Min { get; }

        /// <summary>
        /// Upper bound of the current instance
        /// </summary>
        public ConstantExpression Max { get; }

        /// <summary>
        /// Builds a new <see cref="RangeExpression"/> instance
        /// </summary>
        /// <param name="min">Lower bound of the interval</param>
        /// <param name="max">Upper bound of the interval</param>
        /// <exception cref="ArgumentNullException">if both <paramref name="min"/> and <paramref name="max"/> are <c>null</c>.</exception>
        public RangeExpression(ConstantExpression min = null, ConstantExpression max = null)
        {
            if (min is null && max is null)
            {
                throw new ArgumentNullException();
            }
            Min = min;
            Max = max;
        }

        public bool Equals(RangeExpression other) => Equals(Min, other?.Min) && Equals(Max, other?.Max);

        public override bool Equals(object obj) => Equals(obj as RangeExpression);

        public override int GetHashCode() => (Min, Max).GetHashCode();
    }
}
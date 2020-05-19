using System;
using System.Collections.Generic;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// An expression that can be used to construct <see cref="RangeExpression"/> instances.
    /// </summary>
    public class BoundaryExpression : IEquatable<BoundaryExpression>
    {
        /// <summary>
        /// Expression used as a boundary
        /// </summary>
        public IBoundaryExpression Expression { get; }

        /// <summary>
        /// Should the <see cref="Expresssion"/> be included or excluded in the <see cref="RangeExpression"/>
        /// </summary>
        public bool Included { get; }

        /// <summary>
        /// Builds a new <see cref="BoundaryExpression"/> instance.
        /// </summary>
        /// <param name="expression">an <see cref="IBoundaryExpression"/></param>
        /// <param name="included"><c>true</c>if <paramref name="expression"/> should be included in the interval and <c>false</c> otherwise.</param>
        public BoundaryExpression(IBoundaryExpression expression, bool included)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Included = included;
        }

        public bool Equals(BoundaryExpression other) => other != null
                                                        && Expression.Equals(other.Expression)
                                                        && Included == other.Included;

        public override bool Equals(object obj) => Equals(obj as BoundaryExpression);

#if !(NETSTANDARD1_3 || NETSTANDARD2_0)
        public override int GetHashCode() => HashCode.Combine(Expression, Included);
#else
        public override int GetHashCode()
        {
            int hashCode = 1608575900;
            hashCode = hashCode * -1521134295 + EqualityComparer<IBoundaryExpression>.Default.GetHashCode(Expression);
            hashCode = hashCode * -1521134295 + Included.GetHashCode();
            return hashCode;
        }
#endif

        public static bool operator ==(BoundaryExpression left, BoundaryExpression right)
        {
            return EqualityComparer<BoundaryExpression>.Default.Equals(left, right);
        }

        public static bool operator !=(BoundaryExpression left, BoundaryExpression right)
        {
            return !(left == right);
        }

    }
}

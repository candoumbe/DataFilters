namespace DataFilters.Grammar.Syntax
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A <see cref="FilterExpression"/> that can be used to construct <see cref="RangeExpression"/> instances.
    /// </summary>
    public sealed class BoundaryExpression : IEquatable<BoundaryExpression>
    {
        /// <summary>
        /// Expression used as a boundary
        /// </summary>
        public IBoundaryExpression Expression { get; }

        /// <summary>
        /// Should the <see cref="Expression"/> be included or excluded in the <see cref="RangeExpression"/>
        /// </summary>
        public bool Included { get; }

        /// <summary>
        /// Builds a new <see cref="BoundaryExpression"/> instance.
        /// </summary>
        /// <param name="expression">an <see cref="IBoundaryExpression"/></param>
        /// <param name="included"><c>true</c> if <paramref name="expression"/> should be included in the interval and <c>false</c> otherwise.</param>
        public BoundaryExpression(IBoundaryExpression expression, bool included)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Included = included;
        }

        ///<inheritdoc/>
        public bool Equals(BoundaryExpression other) => other != null
                                                        && Included == other.Included
                                                        && Expression.Equals(other.Expression);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as BoundaryExpression);

        ///<inheritdoc/>
#if !(NETSTANDARD1_3 || NETSTANDARD2_0)
        public override int GetHashCode() => HashCode.Combine(Expression, Included);
#else
        public override int GetHashCode()
        {
            int hashCode = 1608575900;
            hashCode = (hashCode * -1521134295) + EqualityComparer<IBoundaryExpression>.Default.GetHashCode(Expression);
            hashCode = (hashCode * -1521134295) + Included.GetHashCode();
            return hashCode;
        }
#endif

        ///<inheritdoc/>
        public static bool operator ==(BoundaryExpression left, BoundaryExpression right) => left switch
        {
            null => right is null,
            _ => left.Equals(right),
        };

        ///<inheritdoc/>
        public static bool operator !=(BoundaryExpression left, BoundaryExpression right) => !(left == right);

        ///<inheritdoc/>
        public override string ToString() => $"{{{Expression.GetType()}:{Expression}, {nameof(Included)} : {Included}}}";
    }
}

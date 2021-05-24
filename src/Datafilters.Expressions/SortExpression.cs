using System;
using System.Linq.Expressions;
using static DataFilters.SortDirection;

namespace DataFilters.Expressions
{
    /// <summary>
    /// An instance of this class holds an <see cref="LambdaExpression"/> which defines a property and its related <see cref="SortDirection"/> to use to order collections
    /// </summary>
    /// <typeparam name="T">Type of the object which the <see cref="OrderClause{T}"/> c</typeparam>
    /// <remarks>
    /// An <see cref="OrderClause{T}"/> only can be created by calling either
    /// <see cref="Create{TProperty}(Expression{Func{T, TProperty}}, SortDirection)"/> or <see cref="Create(LambdaExpression, SortDirection)"/>
    /// </remarks>
    public sealed class OrderClause<T>
    {
        /// <summary>
        /// Builds a new <see cref="OrderClause{T}"/> instance
        /// </summary>
        /// <param name="keySelector">Lambda expression to the property onto which the sort will be performed. </param>
        /// <param name="direction">Direction of the sort.</param>
        public OrderClause(LambdaExpression keySelector, SortDirection direction)
        {
            Expression = keySelector;
            Direction = direction;
        }

        /// <summary>
        /// Creates a new instance of <see cref="OrderClause{T}"/>
        /// </summary>
        /// <typeparam name="TProperty">Type of the property which will serve to order collections' items</typeparam>
        /// <param name="keySelector">Expression used to select the property to used to order items in a collection</param>
        /// <param name="direction">Order direction</param>
        /// <returns>A fully built <see cref="OrderClause{T}"/> instance.</returns>
        public static OrderClause<T> Create<TProperty>(Expression<Func<T, TProperty>> keySelector, SortDirection direction = Ascending) => new(keySelector, direction);

        /// <summary>
        /// Creates a new instance of <see cref="OrderClause{T}"/>
        /// </summary>
        /// <param name="keySelector">Expression used to select the property to used to order items in a collection</param>
        /// <param name="direction">Order direction</param>
        /// <returns>A fully built OrderClause</returns>
        public static OrderClause<T> Create(LambdaExpression keySelector, SortDirection direction = Ascending) => new(keySelector, direction);

        /// <summary>
        /// The lambda expression to use when
        /// </summary>
        public LambdaExpression Expression { get; }

        /// <summary>
        /// Order direction (either <see cref="Ascending"/> or <see cref="Descending"/>
        /// </summary>
        public SortDirection Direction { get; }
    }
}

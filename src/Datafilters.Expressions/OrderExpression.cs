namespace DataFilters.Expressions;

using System;
using System.Linq.Expressions;

using static DataFilters.OrderDirection;

/// <summary>
/// An instance of this class holds an <see cref="LambdaExpression"/> which defines a property and its related <see cref="OrderDirection"/> to use to order collections
/// </summary>
/// <typeparam name="T">Type of the object which the <see cref="OrderExpression{T}"/> c</typeparam>
/// <remarks>
/// An <see cref="OrderExpression{T}"/> only can be created by calling either
/// <see cref="Create{TProperty}(Expression{Func{T, TProperty}}, OrderDirection)"/> or <see cref="Create(LambdaExpression, OrderDirection)"/>
/// </remarks>
/// <remarks>
/// Builds a new <see cref="OrderExpression{T}"/> instance
/// </remarks>
/// <param name="keySelector">Lambda expression to the property onto which the sort will be performed. </param>
/// <param name="direction">Direction of the sort.</param>
public sealed class OrderExpression<T>(LambdaExpression keySelector, OrderDirection direction)
{
    /// <summary>
    /// Creates a new instance of <see cref="OrderExpression{T}"/>
    /// </summary>
    /// <typeparam name="TProperty">Type of the property which will serve to order collections' items</typeparam>
    /// <param name="keySelector">Expression used to select the property to used to order items in a collection</param>
    /// <param name="direction">Order direction</param>
    /// <returns>A fully built <see cref="OrderExpression{T}"/> instance.</returns>
    public static OrderExpression<T> Create<TProperty>(Expression<Func<T, TProperty>> keySelector, OrderDirection direction = Ascending) => new(keySelector, direction);

    /// <summary>
    /// Creates a new instance of <see cref="OrderExpression{T}"/>
    /// </summary>
    /// <param name="keySelector">Expression used to select the property to used to order items in a collection</param>
    /// <param name="direction">Order direction</param>
    /// <returns>A fully built OrderClause</returns>
    public static OrderExpression<T> Create(LambdaExpression keySelector, OrderDirection direction = Ascending) => new(keySelector, direction);

    /// <summary>
    /// The lambda expression to use when
    /// </summary>
    public LambdaExpression Expression { get; } = keySelector;

    /// <summary>
    /// Order direction (either <see cref="Ascending"/> or <see cref="Descending"/>
    /// </summary>
    public OrderDirection Direction { get; } = direction;
}

namespace System.Linq;

using System.Collections.Generic;
using System.Linq.Expressions;
using DataFilters;
using DataFilters.Expressions;

/// <summary>
/// Provides extension methods for ordering a collection of entities.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Orders the <paramref name="entries"/> based on the specified <paramref name="orderBy"/> object.
    /// </summary>
    /// <typeparam name="T">The type of the entities in the collection.</typeparam>
    /// <param name="entries">The collection of entities to be ordered.</param>
    /// <param name="orderBy">The order expression object specifying the ordering criteria.</param>
    /// <returns>An ordered queryable collection of entities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="entries"/> or <paramref name="orderBy"/> is <see langword="null"/>.</exception>
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> entries, in IOrder<T> orderBy)
    {
        // Check for null parameters
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(orderBy);
#else
        if (entries is null)
        {
            throw new ArgumentNullException(nameof(entries));
        }

        if (orderBy is null)
        {
            throw new ArgumentNullException(nameof(orderBy));
        }
#endif

        // Get the list of order expressions
        IEnumerable<OrderExpression<T>> orders = orderBy.ToOrderClause();
        OrderExpression<T> first = orders.First();

        // Build the initial sorting expression
        Expression sortExpression = Expression.Call(typeof(Queryable),
                                                    first.Direction switch
                                                    {
                                                        OrderDirection.Ascending => nameof(Queryable.OrderBy),
                                                        _ => nameof(Queryable.OrderByDescending)
                                                    },
                                                    [entries.ElementType, first.Expression.ReturnType],
                                                    entries.Expression, first.Expression);

        // Build the subsequent sorting expressions
        foreach (OrderExpression<T> order in orders.Skip(1))
        {
            sortExpression = Expression.Call(typeof(Queryable),
                                            order.Direction switch
                                            {
                                                OrderDirection.Ascending => nameof(Queryable.ThenBy),
                                                _ => nameof(Queryable.ThenByDescending)
                                            },
                                            [entries.ElementType, order.Expression.ReturnType],
                                            sortExpression, order.Expression);
        }

        // Create the ordered queryable collection
        return (IOrderedQueryable<T>)entries.Provider.CreateQuery(sortExpression);
    }
}

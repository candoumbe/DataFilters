namespace System.Linq
{
    using DataFilters;
    using DataFilters.Expressions;

    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Contains utility methods for Queryable
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Orders the <paramref name="entries"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entries"></param>
        /// <param name="orderBy">List of <see cref="OrderClause{T}"/></param>
        /// <returns><see cref="IOrderedQueryable{T}"/></returns>
        /// <exception cref="ArgumentNullException">if either <paramref name="entries"/> or <paramref name="orderBy"/> is <c>null</c></exception>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> entries, in ISort<T> orderBy)
        {
            if (entries is null)
            {
                throw new ArgumentNullException(nameof(entries));
            }
            if (orderBy is null)
            {
                throw new ArgumentNullException(nameof(orderBy));
            }

            IEnumerable<OrderClause<T>> orders = orderBy.ToOrderClause();
            OrderClause<T> first = orders.First();

            Expression sortExpression = Expression.Call(typeof(Queryable),
                                                        first.Direction switch
                                                        {
                                                            SortDirection.Ascending => nameof(Queryable.OrderBy),
                                                            _ => nameof(Queryable.OrderByDescending)
                                                        },
                                                        new Type[] { entries.ElementType, first.Expression.ReturnType },
                                                        entries.Expression, first.Expression);

            foreach (var order in orders.Skip(1))
            {
                sortExpression = Expression.Call(typeof(Queryable),
                                                order.Direction switch
                                                {
                                                    SortDirection.Ascending => nameof(Queryable.ThenBy),
                                                    _ => nameof(Queryable.ThenByDescending)
                                                },
                                                new Type[] { entries.ElementType, order.Expression.ReturnType },
                                                sortExpression, order.Expression);
            }

            return (IOrderedQueryable<T>)entries.Provider.CreateQuery(sortExpression);
        }
    }
}

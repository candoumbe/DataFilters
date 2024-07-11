namespace DataFilters
{
    using System;
    using System.Collections.Generic;
    using DataFilters.Expressions;

    /// <summary>
    /// Extension methods for <see cref="IOrder{T}"/> instances.
    /// </summary>
    public static class OrderExtensions
    {
        /// <summary>
        /// Converts <paramref name="sort"/> to a collection of <see cref="OrderExpression{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of element onto the sort expression will target.</typeparam>
        /// <param name="sort"></param>
        /// <returns>a collection of <see cref="OrderExpression{T}"/>s</returns>
        public static IEnumerable<OrderExpression<T>> ToOrderClause<T>(this IOrder<T> sort)
        {
            if (sort is not Order<T> and not MultiOrder<T>)
            {
                throw new ArgumentOutOfRangeException(nameof(sort), "Unknown sort expression");
            }

            return BuildOrderClauses(sort);

            static IEnumerable<OrderExpression<T>> BuildOrderClauses(IOrder<T> sort)
            {
                switch (sort)
                {
                    case Order<T> singleSort:
                        yield return OrderExpression<T>.Create(singleSort.Expression.ToLambda<T>(), singleSort.Direction);
                        break;
                    case MultiOrder<T> multiSort:
                        foreach (Order<T> item in multiSort.Orders)
                        {
                            foreach (OrderExpression<T> order in item.ToOrderClause())
                            {
                                yield return order;
                            }
                        }
                        break;
                }
            }
        }
    }
}

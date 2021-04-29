
using DataFilters.Expressions;
using System;
using System.Collections.Generic;

namespace DataFilters
{
    /// <summary>
    /// Extension methods for <see cref="ISort{T}"/> instances.
    /// </summary>
    public static class SortExtensions
    {
        /// <summary>
        /// Converts <paramref name="sort"/> to a collection of <see cref="OrderClause{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of element onto the sort expression will target.</typeparam>
        /// <param name="sort"></param>
        /// <returns>a collection of <see cref="OrderClause{T}"/>s</returns>
        public static IEnumerable<OrderClause<T>> ToOrderClause<T>(this ISort<T> sort)
        {
            if (sort is not Sort<T> and not MultiSort<T>)
            {
                throw new ArgumentOutOfRangeException(nameof(sort), "Unknown sort expression");
            }

            return BuildOrderClauses(sort);

            static IEnumerable<OrderClause<T>> BuildOrderClauses(ISort<T> sort)
        {
            switch (sort)
            {
                case Sort<T> singleSort:
                    yield return OrderClause<T>.Create(singleSort.Expression.ToLambda<T>(), singleSort.Direction);
                    break;
                case MultiSort<T> multiSort:
                    foreach (Sort<T> item in multiSort.Sorts)
                    {
                        foreach (OrderClause<T> order in item.ToOrderClause())
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


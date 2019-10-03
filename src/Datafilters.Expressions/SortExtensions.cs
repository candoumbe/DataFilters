
using DataFilters.Expressions;
using System;
using System.Collections.Generic;

namespace DataFilters
{
    public static class SortExtensions
    {
        /// <summary>
        /// Converts a 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static IEnumerable<OrderClause<T>> ToOrderClause<T>(this ISort<T> sort)
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(sort), "Unknown sort expression");
            }
        }
    }
}


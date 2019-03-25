
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
            List<OrderClause<T>> orders = new List<OrderClause<T>>();

            switch (sort)
            {
                case Sort<T> singleSort:
                    orders.Add(OrderClause<T>.Create(singleSort.Expression.ToLambda<T>(), singleSort.Direction));
                    break;
                case MultiSort<T> multiSort:
                    foreach (Sort<T> item in multiSort.Sorts)
                    {
                        orders.AddRange(item.ToOrderClause());
                    }
                    break;

            }

            return orders;
        }
    }
}


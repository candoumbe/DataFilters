using Queries.Core.Parts.Sorting;
using System;
using System.Collections.Generic;

namespace DataFilters
{
    /// <summary>
    /// Extensions method for <see cref="ISort{T}"/> types
    /// </summary>
    public static class SortExtensions
    {
        /// <summary>
        /// Converts <paramref name="sort"/> to <see cref="IOrder"/>.
        /// </summary>
        /// <typeparam name="T">Type of element the sort will be apply to.</typeparam>
        /// <param name="sort"></param>
        /// <exception cref="NotSupportedException"><paramref name="sort"/> is neither <see cref="Sort{T}"/> nor <see cref="MultiSort{T}"/>.</exception>
        public static IEnumerable<IOrder> ToOrder<T>(this ISort<T> sort)
        {
            static OrderExpression CreateOrderExpressionFromSort(in Sort<T> instance)
            {
                return new OrderExpression(instance.Expression.Field(), direction: instance.Direction == SortDirection.Ascending
                    ? OrderDirection.Ascending
                    : OrderDirection.Descending);
            }

            switch (sort)
            {
                case Sort<T> expression:
                    yield return CreateOrderExpressionFromSort(expression);
                    break;
                case MultiSort<T> multisort:
                    {
                        foreach (Sort<T> item in multisort.Sorts)
                        {
                            yield return CreateOrderExpressionFromSort(item);
                        }

                        break;
                    }

                default:
                    throw new NotSupportedException("Unsupported sort type");
            }
        }
    }
}

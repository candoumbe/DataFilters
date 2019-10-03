using Queries.Core.Parts.Sorting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataFilters
{
    public static class SortToQueries
    {
        /// <summary>
        /// Converts <paramref name="sort"/> to <see cref="IOrder"/>.
        /// </summary>
        /// <typeparam name="T">Type of element the sort will be apply to.</typeparam>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static IEnumerable<IOrder> ToOrder<T>(this ISort<T> sort)
        {
            static OrderExpression CreateOrderExpressionFromSort(in Sort<T> instance)
            {
                return new OrderExpression(instance.ToString().Field(), direction: instance.Direction == SortDirection.Ascending
                    ? OrderDirection.Ascending : OrderDirection.Descending);
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
                    throw new ArgumentOutOfRangeException(nameof(sort), "Unknown sort type");
            }
        }
    }
}

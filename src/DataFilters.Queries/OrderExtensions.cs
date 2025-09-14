
using System;
using System.Collections.Generic;
using Queries.Core.Parts.Sorting;

// ReSharper disable once CheckNamespace
namespace DataFilters;
/// <summary>
/// Extensions method for <see cref="IOrder{T}"/> types
/// </summary>
public static class OrderExtensions
{
    /// <summary>
    /// Converts <paramref name="order"/> to <see cref="IOrder"/>.
    /// </summary>
    /// <typeparam name="T">Type of element the sort will be apply to.</typeparam>
    /// <param name="order"></param>
    /// <exception cref="NotSupportedException"><paramref name="order"/> is neither <see cref="Order{T}"/> nor <see cref="MultiOrder{T}"/>.</exception>
    public static IEnumerable<IOrder> ToOrder<T>(this IOrder<T> order)
    {
        switch (order)
        {
            case Order<T> expression:
                yield return CreateOrderExpressionFromOrder(expression);
                break;
            case MultiOrder<T> multiSort:
                {
                    foreach (Order<T> item in multiSort.Orders)
                    {
                        yield return CreateOrderExpressionFromOrder(item);
                    }

                    break;
                }

            default:
                throw new NotSupportedException("Unsupported order type");
        }

        yield break;

        static OrderExpression CreateOrderExpressionFromOrder(in Order<T> instance)
        {
            return new OrderExpression(instance.Expression.Field(), direction: instance.Direction == OrderDirection.Ascending
                                                                                   ? Queries.Core.Parts.Sorting.OrderDirection.Ascending
                                                                                   : Queries.Core.Parts.Sorting.OrderDirection.Descending);
        }
    }
}
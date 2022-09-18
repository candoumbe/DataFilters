namespace DataFilters
{
    using Queries.Core.Parts.Sorting;

    using System;
    using System.Collections.Generic;

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
            static OrderExpression CreateOrderExpressionFromOrder(in Order<T> instance)
            {
                return new OrderExpression(instance.Expression.Field(), direction: instance.Direction == OrderDirection.Ascending
                    ? Queries.Core.Parts.Sorting.OrderDirection.Ascending
                    : Queries.Core.Parts.Sorting.OrderDirection.Descending);
            }

            switch (order)
            {
                case Order<T> expression:
                    yield return CreateOrderExpressionFromOrder(expression);
                    break;
                case MultiOrder<T> multisort:
                    {
                        foreach (Order<T> item in multisort.Orders)
                        {
                            yield return CreateOrderExpressionFromOrder(item);
                        }

                        break;
                    }

                default:
                    throw new NotSupportedException("Unsupported order type");
            }
        }
    }
}

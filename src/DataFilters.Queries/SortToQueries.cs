using Queries.Core.Parts.Sorting;
using System;
using System.Reflection;

namespace DataFilters
{
    public static class SortToQueries
    {
        /// <summary>
        /// Converts <paramref name="sort"/> to <see cref="IOrder"/>.
        /// </summary>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static IOrder ToOrder<T>(this ISort<T> sort)
        {
            IOrder order = null;

            if (sort.GetType().IsAssignableToGenericType(typeof(Sort<>)))
            {
                PropertyInfo piExpression = sort.GetType().GetProperty(nameof(Sort<object>.Expression));
                object expression = piExpression.GetValue(sort).ToString();

                PropertyInfo piDirection = sort.GetType().GetProperty(nameof(Sort<object>.Direction));
                SortDirection direction = (SortDirection) piDirection.GetValue(sort);

                order = new OrderExpression(expression.ToString().Field(), direction: direction == SortDirection.Ascending
                    ? OrderDirection.Ascending : OrderDirection.Descending);
            }
            else
            {

            }

            return order;
        }
    }
}

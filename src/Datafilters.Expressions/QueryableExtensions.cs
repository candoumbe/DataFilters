using Datafilters.Expressions;
using DataFilters;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Linq
{
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
        /// <returns></returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> entries, in IEnumerable<OrderClause<T>> orderBy)
        {
            OrderClause<T> previousClause = null;
            IOrderedQueryable<T> ordered = null;
            foreach (OrderClause<T> orderClause in orderBy)
            {
                switch (orderClause.Direction)
                {
                    case SortDirection.Ascending:
                        ordered = previousClause != null
                            ? Queryable.ThenBy(ordered, (dynamic)orderClause.Expression)
                            : Queryable.OrderBy(entries, (dynamic)orderClause.Expression);
                        break;
                    case SortDirection.Descending:
                        ordered = previousClause != null
                                ? Queryable.ThenByDescending(ordered, (dynamic)orderClause.Expression)
                                : Queryable.OrderByDescending(entries, (dynamic)orderClause.Expression);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                previousClause = orderClause;
            }

            Debug.Assert(ordered != null);
            
            return ordered;
        }
    }
}

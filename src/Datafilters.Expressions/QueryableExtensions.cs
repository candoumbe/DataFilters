using DataFilters.Expressions;
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
        /// <returns><see cref="IOrderedQueryable{T}"/></returns>
        /// <exception cref="ArgumentNullException">if either <paramref name="entries"/> or <paramref name="orderBy"/> is <c>null</c></exception>
        [Obsolete("Use OrderBy<T>(IQueryable<T> entries, ISort<T> orderBy) instead")]
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> entries, in IEnumerable<OrderClause<T>> orderBy)
        {
            if (orderBy == null)
            {
                throw new ArgumentNullException(nameof(orderBy));
            }

            if (!orderBy.Any())
            {
                throw new EmptyOrderByException();
            }
            OrderClause<T> first = orderBy.First();
            IOrderedQueryable<T> ordered = first.Direction == SortDirection.Ascending 
                ? Queryable.OrderBy(entries, (dynamic)first.Expression)
                : Queryable.OrderByDescending(entries, (dynamic)first.Expression);

            foreach (OrderClause<T> orderClause in orderBy.Skip(1))
            {
                switch (orderClause.Direction)
                {
                    case SortDirection.Ascending:
                        ordered = Queryable.ThenBy(ordered, (dynamic)orderClause.Expression);
                        break;
                    case SortDirection.Descending:
                        ordered = Queryable.ThenByDescending(ordered, (dynamic)orderClause.Expression);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }            
            return ordered;
        }

        /// <summary>
        /// Orders the <paramref name="entries"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entries"></param>
        /// <param name="orderBy">List of <see cref="OrderClause{T}"/></param>
        /// <returns><see cref="IOrderedQueryable{T}"/></returns>
        /// <exception cref="ArgumentNullException">if either <paramref name="entries"/> or <paramref name="orderBy"/> is <c>null</c></exception>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> entries, in ISort<T> orderBy)
        {
            if (orderBy == null)
            {
                throw new ArgumentNullException(nameof(orderBy));
            }

            IEnumerable<OrderClause<T>> orders = orderBy.ToOrderClause();
            OrderClause<T> first = orders.First();
            IOrderedQueryable<T> ordered = first.Direction == SortDirection.Ascending
                ? Queryable.OrderBy(entries, (dynamic)first.Expression)
                : Queryable.OrderByDescending(entries, (dynamic)first.Expression);

            foreach (OrderClause<T> orderClause in orders.Skip(1))
            {
                switch (orderClause.Direction)
                {
                    case SortDirection.Ascending:
                        ordered = Queryable.ThenBy(ordered, (dynamic)orderClause.Expression);
                        break;
                    case SortDirection.Descending:
                        ordered = Queryable.ThenByDescending(ordered, (dynamic)orderClause.Expression);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return ordered;
        }
    }
}

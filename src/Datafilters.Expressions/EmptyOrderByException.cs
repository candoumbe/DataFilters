using System;
using System.Collections.Generic;
using System.Linq;

namespace DataFilters.Expressions
{
    /// <summary>
    /// Exception thrown when calling <see cref="QueryableExtensions.OrderBy{T}(IQueryable{T}, in IEnumerable{OrderClause{T}})"/> with
    /// an empty orderBy
    /// </summary>
    public class EmptyOrderByException : ArgumentException
    {
    }
}

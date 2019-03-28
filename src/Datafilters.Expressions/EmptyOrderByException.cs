using System;
using System.Collections.Generic;
using System.Linq;

namespace DataFilters.Expressions
{
    /// <summary>
    /// Exception thrown when calling <see cref="QueryableExtensions.OrderBy{T}(IQueryable{T}, in IEnumerable{OrderClause{T}})"/> with
    /// an empty orderBy
    /// </summary>
    [Obsolete("Will be removed in a future release")]
    public class EmptyOrderByException : ArgumentException
    {
    }
}

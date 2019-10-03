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
        public EmptyOrderByException() : base()
        {
        }

        public EmptyOrderByException(string message) : base(message)
        {
        }

        public EmptyOrderByException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EmptyOrderByException(string message, string paramName) : base(message, paramName)
        {
        }

        public EmptyOrderByException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
        {
        }

#if !NETSTANDARD1_3
        protected EmptyOrderByException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}

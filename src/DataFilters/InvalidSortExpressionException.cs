using System;

namespace DataFilters
{
    /// <summary>
    /// Exception thrown for an invalid sort expression.
    /// </summary>
    [Serializable]
    public class InvalidSortExpressionException : Exception
    {
        /// <inheritdoc/>
        public InvalidSortExpressionException(string expression) : base($"'{expression}' must matches '{SortValidator.Pattern}' pattern")
        {
        }

        /// <inheritdoc/>
        public InvalidSortExpressionException() : base()
        {
        }

#if NETSTANDARD2_0 || NETSTANDARD2_1
        /// <inheritdoc/>
        protected InvalidSortExpressionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
#endif

        /// <inheritdoc/>
        public InvalidSortExpressionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

using System;
using System.Runtime.Serialization;

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

        /// <inheritdoc/>
        public InvalidSortExpressionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

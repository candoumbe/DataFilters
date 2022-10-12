namespace DataFilters
{
    using System;

    /// <summary>
    /// Exception thrown for an invalid sort expression.
    /// </summary>
    [Serializable]
    public class InvalidOrderExpressionException : Exception
    {
        /// <inheritdoc/>
        public InvalidOrderExpressionException(string expression) : base($"'{expression}' must matches '{OrderValidator.Pattern}' pattern")
        {
        }

        /// <inheritdoc/>
        public InvalidOrderExpressionException() : base()
        {
        }

        /// <inheritdoc/>
        public InvalidOrderExpressionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

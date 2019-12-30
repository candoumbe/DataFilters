using System;

namespace DataFilters.Grammar.Exceptions
{
    /// <summary>
    /// Exception thrown when <see cref="RangeExpression"/> constructor is passed an incorrect object
    /// </summary>
    public class IncorrectBoundaryException : ArgumentOutOfRangeException
    {
        public IncorrectBoundaryException() : base()
        {
        }

        public IncorrectBoundaryException(string paramName) : base(paramName)
        {
        }

        public IncorrectBoundaryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public IncorrectBoundaryException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
        {
        }

        public IncorrectBoundaryException(string paramName, string message) : base(paramName, message)
        {
        }
    }
}

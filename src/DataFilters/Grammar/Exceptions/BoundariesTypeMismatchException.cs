using System;

namespace DataFilters.Grammar.Exceptions
{
    public class BoundariesTypeMismatchException : ArgumentException
    {
        public BoundariesTypeMismatchException()
        {

        }

        public BoundariesTypeMismatchException(string message) : base(message)
        {
        }

        public BoundariesTypeMismatchException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public BoundariesTypeMismatchException(string message, string paramName) : base(message, paramName)
        {
        }

        public BoundariesTypeMismatchException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
        {
        }
    }
}

#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Runtime.Serialization;
using System;

namespace DataFilters;

/// <summary>
/// Exception thrown whenever a property of <see cref="FilterServiceOptions"/> is set with a invalid value.
/// </summary>
[Serializable]
public class FilterServiceOptionsInvalidValueException : Exception
{
    ///<inheritdoc/>
    public FilterServiceOptionsInvalidValueException()
    {
    }

    ///<inheritdoc/>
    public FilterServiceOptionsInvalidValueException(string message) : base(message)
    {
    }

    ///<inheritdoc/>
    public FilterServiceOptionsInvalidValueException(string message, Exception innerException) : base(message, innerException)
    {
    }

    ///<inheritdoc/>
    protected FilterServiceOptionsInvalidValueException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
#endif
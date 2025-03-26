namespace DataFilters.Grammar.Exceptions;

using System;

/// <summary>
/// Exception thrown when <see cref="Syntax.IntervalExpression"/> constructor is passed incorrect <see cref="Syntax.BoundaryExpression"/>s.
/// </summary>
public class IncorrectBoundaryException : ArgumentException
{
    ///<inheritdoc/>
    public IncorrectBoundaryException() : base()
    {
    }

    ///<inheritdoc/>
    public IncorrectBoundaryException(string message) : base(message)
    {
    }

    ///<inheritdoc/>
    public IncorrectBoundaryException(string message, Exception innerException) : base(message, innerException)
    {
    }

    ///<inheritdoc/>
    public IncorrectBoundaryException(string message, string paramName) : base(message, paramName)
    {
    }

    ///<inheritdoc/>
    public IncorrectBoundaryException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
    {
    }
}
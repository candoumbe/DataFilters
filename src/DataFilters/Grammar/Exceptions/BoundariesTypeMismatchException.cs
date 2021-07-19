﻿namespace DataFilters.Grammar.Exceptions
{
    using System;

    /// <summary>
    /// Exception thrown when a <see cref="Syntax.IntervalExpression"/> has incorrect <see cref="Syntax.IntervalExpression.Min"/>
    /// </summary>
#pragma warning disable RCS1194 // Implement exception constructors.
    [Serializable]
    public sealed class BoundariesTypeMismatchException : ArgumentException
#pragma warning restore RCS1194 // Implement exception constructors.
    {
        /// <summary>
        /// Creates a new <see cref="BoundariesTypeMismatchException"/> instance
        /// </summary>
        /// <param name="message">message of the exception</param>
        /// <param name="paramName">name of the argument which causes the exception to be thrown</param>
        public BoundariesTypeMismatchException(string message, string paramName) : base(message, paramName)
        {
        }
    }
}

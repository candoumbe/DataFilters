using DataFilters.ValueObjects;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Defines <see cref="OriginalString"/> property
    /// </summary>
    public interface IProvideOriginalString
    {
        /// <summary>
        /// The string which was used to build the <see cref="FilterExpression"/>.
        /// </summary>
        RawString OriginalString { get; }
    }
}
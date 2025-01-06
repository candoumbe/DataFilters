using DataFilters.ValueObjects;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Defines <see cref="EscapedParseableString"/> property.
    /// </summary>
    public interface IProvideParseableString
    {
        /// <summary>
        /// The string that, if parsed, will give the current expression.
        /// </summary>
        EscapedString EscapedParseableString { get; }
    }
}
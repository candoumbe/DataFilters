namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Defines <see cref="ParseableString"/> property.
    /// </summary>
    public interface IParseableString
    {
        /// <summary>
        /// The string that, if parsed, will give the current expression
        /// </summary>
        string ParseableString { get; }
    }
}
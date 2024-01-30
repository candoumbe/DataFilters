namespace DataFilters.Grammar.Syntax;

/// <summary>
/// Defines <see cref="EscapedParseableString"/> property.
/// </summary>
public interface IParseableString
{
    /// <summary>
    /// The string that, if parsed, will give the current expression
    /// </summary>
    string EscapedParseableString { get; }

    /// <summary>
    /// Unescaped version of the parseable string.
    /// </summary>
    /// <remarks>
    /// This is the raw version which <see cref="EscapedParseableString"/> is computed from.
    /// </remarks>
    string OriginalString { get; }
}
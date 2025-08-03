namespace DataFilters.Grammar.Parsing;

/// <summary>
/// Defines the way <see cref="FilterTokenizer"/> "tokenizes" each <see cref="char"/> read.
/// </summary>
public enum TokenizerMode
{
    /// <summary>
    /// Character are interpreted as-is
    /// </summary>
    Normal,

    /// <summary>
    /// Each character read is considered as already escaped
    /// </summary>
    Escaped
}
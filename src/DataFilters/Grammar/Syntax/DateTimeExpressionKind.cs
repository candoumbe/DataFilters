namespace DataFilters.Grammar.Syntax;

/// <summary>
/// Defines a type of <see cref="DateTimeExpression"/> instance
/// </summary>
public enum DateTimeExpressionKind
{
    /// <summary>
    /// Default value
    /// </summary>
    Unspecified,

    /// <summary>
    /// Inidcates that the <see cref="DateTimeExpression"/> is a local date/time.
    /// </summary>
    Local,

    /// <summary>
    /// Inidcates that the <see cref="DateTimeExpression"/> is a UTC datetime.
    /// </summary>
    Utc
}

namespace DataFilters.Grammar.Parsing;

using Superpower.Display;

/// <summary>
/// Enumeration of token used throughout the parsing process.
/// </summary>
/// <remarks>
/// <see cref="FilterToken"/>s acts as "markers" with special meaning. They can be combined to created a syntax tree with a higher meaning.
/// </remarks>
public enum FilterToken
{
    /// <summary>
    /// A token that has no specific meaning
    /// </summary>
    None,

    /// <summary>
    /// Start of a group
    /// </summary>
    [Token(Example = "(")]
    OpenParenthese,

    /// <summary>
    /// Token that indicates the end of a group of token.
    /// </summary>
    /// <see cref="OpenParenthese"/>
    [Token(Example = ")")]
    CloseParenthese,

    /// <summary>
    /// Letter
    /// </summary>
    [Token(Example = "c", Description = "a letter")]
    Letter,

    /// <summary>
    /// Numeric value of some sort
    /// </summary>
    [Token(Example = "2", Description = "a digit")]
    Digit,

    /// <summary>
    /// <c>[</c> character
    /// </summary>
    [Token(Example = "(", Description = "right parenthesis")]
    OpenSquaredBracket,

    /// <summary>
    /// <c>]</c> character
    /// </summary>
    [Token(Example = ")", Description = "left parenthesis")]
    CloseSquaredBracket,

    /// <summary>
    /// Asterisk operator used in like expression
    /// </summary>
    [Token(Example = "*", Description = "asterisk")]
    Asterisk,

    /// <summary>
    /// Logical AND operator
    /// </summary>
    [Token(Example = ",", Description = "comma")]
    And,

    /// <summary>
    /// Logical OR operator
    /// </summary>
    [Token(Example = "|", Description = "pipe")]
    Or,

    /// <summary>
    /// Hyphen
    /// </summary>
    [Token(Example = "-", Description = "hyphen")]
    Dash,

    /// <summary>
    /// Equal sign
    /// </summary>
    [Token(Example = "=")]
    Equal,

    /// <summary>
    /// Underscore sign
    /// </summary>
    [Token(Example = "_")]
    Underscore,

    /// <summary>
    /// Bang sign
    /// </summary>
    [Token(Example = "!", Description = "exclamation point")]
    Bang,

    /// <summary>
    /// The whitespace
    /// </summary>
    [Token(Example = " ")]
    Whitespace,

    /// <summary>
    /// The <c>:</c> character
    /// </summary>
    [Token(Example = ":")]
    Colon,

    /// <summary>
    /// The <c>.</c> character.
    /// </summary>
    [Token(Example = ".")]
    Dot,

    /// <summary>
    /// The <c>\</c> (backslash) character
    /// </summary>
    [Token(Example = @"\")]
    Backslash,

    /// <summary>
    /// Token that allow to escape the token that comes right after itself.
    /// </summary>
    [Token(Description = "Token that allow to escape character with a special meaning", Example = @"\\")]
    Escaped,

    /// <summary>
    /// The <c>"</c> charater
    /// </summary>
    [Token(Example = @"""", Description = "double quote")]
    DoubleQuote,

    /// <summary>
    /// The <c>&#38;</c> character.
    /// </summary>
    [Token(Example = "&", Description = "ampersand")]
    Ampersand,

    /// <summary>
    /// The <c>{</c> character.
    /// </summary>
    [Token(Example = "{", Description = "left curly brace")]
    LeftBrace,

    /// <summary>
    /// The <c>}</c> character.
    /// </summary>
    [Token(Example = "}", Description = "right curly brace")]
    RightBrace
}

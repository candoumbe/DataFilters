using Superpower.Display;

namespace DataFilters.Grammar.Parsing;

/// <summary>
/// Enumeration of token used throughout the parsing process.
/// </summary>
/// <remarks>
/// <see cref="FilterToken"/>s acts as "markers" with special meaning. They can be combined to create a syntax tree with a higher meaning.
/// </remarks>
public enum FilterToken
{
    /// <summary>
    /// A token that has no specific meaning
    /// </summary>
    None,

    /// <summary>
    /// Token that indicates the start of a group of token
    /// </summary>
    [Token(Example = "(", Description = "left parenthesis")]
    LeftParenthesis,

    /// <summary>
    /// Token that indicates the end of a group of tokens.
    /// </summary>
    /// <see cref="LeftParenthesis"/>
    [Token(Example = ")", Description = "right parenthesis")]
    RightParenthesis,

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
    [Token(Example = "[", Description = "left squared bracket")]
    LeftSquaredBracket,

    /// <summary>
    /// <c>]</c> character
    /// </summary>
    [Token(Example = "]", Description = "right squared bracket")]
    RightSquaredBracket,

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
    [Token(Example = "=", Description = "equal symbol")]
    Equal,

    /// <summary>
    /// Underscore sign
    /// </summary>
    [Token(Example = "_", Description = "underscore")]
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
    LeftCurlyBrace,

    /// <summary>
    /// The <c>}</c> character.
    /// </summary>
    [Token(Example = "}", Description = "right curly brace")]
    RightCurlyBrace
}
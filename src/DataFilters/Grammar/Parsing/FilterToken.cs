using Superpower.Display;

namespace DataFilters.Grammar.Parsing
{
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
        Letter,

        /// <summary>
        /// Numeric value of some sort
        /// </summary>
        Numeric,

        /// <summary>
        /// <c>[</c> character
        /// </summary>
        OpenSquaredBracket,

        /// <summary>
        /// <c>]</c> character
        /// </summary>
        CloseSquaredBracket,

        /// <summary>
        /// Asterisk operator used in like expression
        /// </summary>
        [Token(Example = "*")]
        Asterisk,

        /// <summary>
        /// Logical AND operator
        /// </summary>
        [Token(Example = ",")]
        And,

        /// <summary>
        /// Logical OR operator
        /// </summary>
        [Token(Example = "|")]
        Or,

        /// <summary>
        /// Hyphen
        /// </summary>
        [Token(Example = "-")]
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
        [Token(Example = "!")]
        Not,

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
        [Token(Example = @"""")]
        DoubleQuote
    }
}

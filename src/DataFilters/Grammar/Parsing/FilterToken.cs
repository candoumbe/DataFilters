using Superpower.Display;

namespace DataFilters.Grammar.Parsing
{
    public enum FilterToken
    {
        /// <summary>
        /// Default token
        /// </summary>
        None,

        /// <summary>
        /// Start of a group
        /// </summary>
        [Token(Example = "(")]
        OpenParenthese,

        /// <summary>
        /// End of a group
        /// </summary>
        [Token(Example = ")")]
        CloseParenthese,

        /// <summary>
        /// Literal
        /// </summary>
        Alpha,

        /// <summary>
        /// Numeric value of some sort
        /// </summary>
        Numeric,

        /// <summary>
        /// '[' character
        /// </summary>
        OpenSquaredBracket,

        /// <summary>
        /// ']' character
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

        [Token(Example = ":")]
        Colon,

        /// <summary>
        /// The dot character
        /// </summary>
        [Token(Example = ".")]
        Dot
    }
}

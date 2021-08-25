namespace DataFilters.Grammar.Parsing
{
    using Superpower;
    using Superpower.Model;

    using System.Collections.Generic;
    using System.Linq;

    using static DataFilters.Grammar.Parsing.FilterToken;

    /// <summary>
    /// <see cref="FilterTokenizer"/> is the base class used to "tokenize" a string.
    /// </summary>
    /// <remarks>
    /// The tokenizer reads a string input and associated to one or more character a <see cref="FilterToken"/> which is the first step in the process of building <see cref="IFilter"/> instances.
    /// </remarks>
    public class FilterTokenizer : Tokenizer<FilterToken>
    {
        /// <summary>
        /// The <c>_</c> character
        /// </summary>
        public const char Underscore = '_';

        /// <summary>
        /// The <c>*</c> character
        /// </summary>
        public const char Asterisk = '*';

        /// <summary>
        /// The <c>=</c> character
        /// </summary>
        public const char EqualSign = '=';

        /// <summary>
        /// The <c>(</c> character
        /// </summary>
        public const char LeftParenthesis = '(';

        /// <summary>
        /// The <c>)</c> character
        /// </summary>
        public const char RightParenthesis = ')';

        /// <summary>
        /// The <c>[</c> character
        /// </summary>
        public const char LeftSquareBracket = '[';

        /// <summary>
        /// The <c>]</c> character
        /// </summary>
        public const char RightSquareBracket = ']';

        /// <summary>
        /// The <c>-</c> character
        /// </summary>
        public const char Hyphen = '-';

        /// <summary>
        /// The <c>\</c> character
        /// </summary>
        public const char BackSlash = '\\';

        /// <summary>
        /// The <c>|</c> character
        /// </summary>
        public const char Pipe = '|';

        /// <summary>
        /// The <c>,</c> character
        /// </summary>
        public const char Comma = ',';

        /// <summary>
        /// The <c>!</c> character
        /// </summary>
        public const char Bang = '!';

        /// <summary>
        /// The <c>"</c> character
        /// </summary>
        public const char DoubleQuote = '"';
        /// <summary>
        /// The <c>&#38;</c> character
        /// </summary>
        public const char Ampersand = '&';

        /// <summary>
        /// List of characters that have a special meaning and should be escaped
        /// </summary>
        public static readonly char[] SpecialCharacters =
        {
            Asterisk,
            EqualSign,
            LeftParenthesis,
            RightParenthesis,
            LeftSquareBracket,
            RightSquareBracket,
            BackSlash,
            Pipe,
            Comma,
            Bang,
            DoubleQuote,
            Ampersand,
            ':',
            '-',
            '.',
            ' '
        };

        ///<inheritdoc/>
        protected override IEnumerable<Result<FilterToken>> Tokenize(TextSpan span, TokenizationState<FilterToken> state)
        {
            Result<char> next = span.ConsumeChar();

            if (!next.HasValue)
            {
                yield break;
            }

            do
            {
                switch (next.Value)
                {
                    case char c when char.IsLetter(c):
                        yield return Result.Value(Letter, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case char c when char.IsDigit(c):
                        yield return Result.Value(Digit, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case Underscore:
                        yield return Result.Value(FilterToken.Underscore, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case Pipe:
                        yield return Result.Value(Or, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case Comma:
                        yield return Result.Value(And, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case EqualSign:
                        yield return Result.Value(Equal, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case Asterisk:
                        yield return Result.Value(FilterToken.Asterisk, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case '!':
                        yield return Result.Value(FilterToken.Bang, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case LeftSquareBracket:
                        yield return Result.Value(OpenSquaredBracket, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case RightSquareBracket:
                        yield return Result.Value(CloseSquaredBracket, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case Hyphen:
                        yield return Result.Value(Dash, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case LeftParenthesis:
                        yield return Result.Value(OpenParenthese, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case RightParenthesis:
                        yield return Result.Value(CloseParenthese, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case ' ':
                        yield return Result.Value(Whitespace, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case ':':
                        yield return Result.Value(Colon, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case '&':
                        yield return Result.Value(FilterToken.Ampersand, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case '"':
                        yield return Result.Value(FilterToken.DoubleQuote, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case '.':
                        yield return Result.Value(Dot, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case BackSlash:
                        TextSpan backSlashStart = next.Location;
                        next = next.Remainder.ConsumeChar();
                        yield return next.HasValue && SpecialCharacters.Contains(next.Value)
                            ? Result.Value(Escaped, next.Location, next.Remainder)
                            : Result.Value(Letter, backSlashStart, next.Remainder);

                        next = next.Remainder.ConsumeChar();
                        break;
                    default:
                        yield return Result.Value(None, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                }

            } while (next.HasValue);
        }
    }
}

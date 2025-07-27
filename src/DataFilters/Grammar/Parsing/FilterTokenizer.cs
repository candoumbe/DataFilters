using System;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using System.Collections.Generic;
using System.Linq;
using Superpower;
using Superpower.Model;
using static DataFilters.Grammar.Parsing.FilterToken;


namespace DataFilters.Grammar.Parsing
{

    /// <summary>
    /// <see cref="FilterTokenizer"/> is the base class used to "tokenize" a string.
    /// </summary>
    /// <remarks>
    /// The tokenizer reads a string input and associated to one or more character a <see cref="FilterToken"/> which is the first step in the process of building <see cref="IFilter"/> instances.
    /// </remarks>
    public class FilterTokenizer : Tokenizer<FilterToken>
    {
        private TokenizerMode _mode;

        /// <summary>
        /// The <c>_</c> character
        /// </summary>
        private const char Underscore = '_';

        /// <summary>
        /// The <c>*</c> character
        /// </summary>
        private const char Asterisk = '*';

        internal const string AsteriskString = "*";

        /// <summary>
        /// The <c>=</c> character
        /// </summary>
        private const char EqualSign = '=';

        /// <summary>
        /// The <c>(</c> character
        /// </summary>
        private const char LeftParenthesis = '(';

        /// <summary>
        /// The <c>)</c> character
        /// </summary>
        private const char RightParenthesis = ')';

        /// <summary>
        /// The <c>[</c> character
        /// </summary>
        private const char LeftSquareBracket = '[';

        /// <summary>
        /// The <c>{</c> character
        /// </summary>
        private const char LeftCurlyBracket = '{';

        /// <summary>
        /// The <c>}</c> character
        /// </summary>
        private const char RightCurlyBracket = '}';

        /// <summary>
        /// The <c>]</c> character
        /// </summary>
        private const char RightSquareBracket = ']';

        /// <summary>
        /// The <c>-</c> character
        /// </summary>
        private const char Hyphen = '-';

        /// <summary>
        /// The <c>\</c> character
        /// </summary>
        public const char BackSlash = '\\';

        /// <summary>
        /// The <c>|</c> character
        /// </summary>
        private const char Pipe = '|';

        /// <summary>
        /// The <c>,</c> character
        /// </summary>
        private const char Comma = ',';

        /// <summary>
        /// The <c>!</c> character
        /// </summary>
        private const char Bang = '!';

        /// <summary>
        /// The <c>"</c> character
        /// </summary>
        public const char DoubleQuote = '"';

        /// <summary>
        /// The <c>"</c> character as a string value
        /// </summary>
        internal const string DoubleQuoteString = @"\""";

        /// <summary>
        /// The <c>&#38;</c> character
        /// </summary>
        private const char Ampersand = '&';

        /// <summary>
        /// The <c>.</c> character.
        /// </summary>
        private const char Dot = '.';

        /// <summary>
        /// The space character.
        /// </summary>
        private const char Space = ' ';

        private const char Colon = ':';

        /// <summary>
        /// The character to use to escape a special character.
        /// </summary>
        public const char EscapedCharacter = BackSlash;

        /// <summary>
        /// List of characters that have a special meaning and should be escaped
        /// </summary>
        public static readonly char[] SpecialCharacters =
        [
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
            RightCurlyBracket,
            LeftCurlyBracket,
            Colon,
            Hyphen,
            Dot,
            Space
        ];

        internal static readonly IReadOnlyDictionary<char, ReadOnlyMemory<char>> EscapedSpecialCharacters = SpecialCharacters
            .ToDictionary(chr => chr, chr => $"{BackSlash}{chr}".AsMemory())
#if NET8_0_OR_GREATER
            .ToFrozenDictionary()
#endif
            ;

        /// <summary>
        /// Custom <see cref="Tokenizer{TKind}"/> implementation that serves as the foundation of parsing text.
        /// </summary>
        public FilterTokenizer() => _mode = TokenizerMode.Normal;

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
                    case var c when char.IsLetter(c):
                    {
                        yield return Result.Value(Letter, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }

                    case var c when char.IsDigit(c):
                    {
                        yield return Result.Value(Digit, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case Underscore:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => FilterToken.Underscore, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case Pipe:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => Or, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case Comma:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => And, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case EqualSign:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => Equal, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case Asterisk:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => FilterToken.Asterisk, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case LeftCurlyBracket:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => LeftCurlyBrace, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case RightCurlyBracket:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => RightCurlyBrace, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case Bang:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => FilterToken.Bang, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case LeftSquareBracket:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => LeftSquaredBracket, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case RightSquareBracket:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => RightSquaredBracket, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case Hyphen:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => Dash, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case LeftParenthesis:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => FilterToken.LeftParenthesis, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case RightParenthesis:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => FilterToken.RightParenthesis, _ => Escaped },
                                                  next.Location,
                                                  next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case Space:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => Whitespace, _ => Escaped },
                                                  next.Location,
                                                  next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case Colon :
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => FilterToken.Colon, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case Ampersand:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => FilterToken.Ampersand, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case DoubleQuote:
                    {
                        yield return Result.Value(FilterToken.DoubleQuote,
                            next.Location,
                            next.Remainder);
                        _mode = ToggleMode(_mode);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case Dot:
                    {
                        yield return Result.Value(_mode switch { TokenizerMode.Normal => FilterToken.Dot, _ => Escaped },
                            next.Location,
                            next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                    case BackSlash:
                    {
                        TextSpan backSlashStart = next.Location;
                        if (_mode == TokenizerMode.Normal)
                        {
                            next = next.Remainder.ConsumeChar();
                            yield return next.HasValue && SpecialCharacters.Contains(next.Value)
                                ? Result.Value(Escaped, next.Location, next.Remainder)
                                : Result.Value(Letter, backSlashStart, next.Remainder);

                            next = next.Remainder.ConsumeChar();
                        }
                        else
                        {
                            TextSpan remainderAfterBackslash = next.Remainder;
                            next = next.Remainder.ConsumeChar();
                            // Only backslash and double quote need to be escaped when in Escaped mode
                            bool shouldEscape = next.Value is BackSlash or DoubleQuote;
                            if (next.HasValue)
                            {
                                if (shouldEscape)
                                {
                                    yield return Result.Value(Escaped, next.Location, next.Remainder);
                                    next = next.Remainder.ConsumeChar();
                                }
                                else
                                {
                                    yield return Result.Value(Escaped, backSlashStart, remainderAfterBackslash);
                                }
                            }
                            else
                            {
                                yield return Result.Value(Escaped, backSlashStart, remainderAfterBackslash);
                            }
                        }

                        break;
                    }
                    default:
                    {
                        yield return Result.Value(None, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    }
                }
            } while (next.HasValue);

            yield break;

            static TokenizerMode ToggleMode(TokenizerMode currentMode) => currentMode switch
            {
                TokenizerMode.Normal => TokenizerMode.Escaped,
                _ => TokenizerMode.Normal
            };
        }
    }
}
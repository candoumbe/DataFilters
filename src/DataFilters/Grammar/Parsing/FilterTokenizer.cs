using Superpower;
using Superpower.Model;
using System.Collections.Generic;
using System.Linq;
using static DataFilters.Grammar.Parsing.FilterToken;

namespace DataFilters.Grammar.Parsing
{
    public class FilterTokenizer : Tokenizer<FilterToken>
    {
        public const char Underscore = '_';
        public const char Asterisk = '*';
        public const char EqualSign = '=';
        public const char LeftParenthesis = '(';
        public const char RightParenthesis = ')';
        public const char LeftSquareBracket = '[';
        public const char RightSquareBracket = ']';
        public const char Hyphen = '-';
        public const char BackSlash = '\\';
        public const char Pipe = '|';
        public const char Comma = ',';
        public const char Bang = '!';

        /// <summary>
        /// List of characters that have a special meaning and should be escaped
        /// </summary>
        public static char[] SpecialCharacters => new[]
        {
            Asterisk,
            EqualSign,
            LeftParenthesis,
            RightParenthesis,
            LeftSquareBracket,
            RightSquareBracket,
            BackSlash,
            Pipe,
            Bang
        };


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
                        {
                            TextSpan identifierStart = next.Location;
                            while (next.HasValue && char.IsLetter(next.Value))
                            {
                                next = next.Remainder.ConsumeChar();
                            }

                            yield return Result.Value(Alpha, identifierStart, next.Location);
                        }
                        break;
                    case char c when char.IsDigit(c):
                        {
                            TextSpan numberStart = next.Location;
                            while (next.HasValue && char.IsDigit(next.Value))
                            {
                                next = next.Remainder.ConsumeChar();
                            }

                            yield return Result.Value(Numeric, numberStart, next.Location);
                        }
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
                        yield return Result.Value(Not, next.Location, next.Remainder);
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
                    case '.':
                        yield return Result.Value(Dot, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case BackSlash:
                        TextSpan backSlashStart = next.Location;
                        next = next.Remainder.ConsumeChar();
                        yield return next.HasValue && SpecialCharacters.Contains(next.Value)
                            ? Result.Value(Escaped, next.Location, next.Remainder)
                            : Result.Value(Alpha, backSlashStart, next.Remainder);

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

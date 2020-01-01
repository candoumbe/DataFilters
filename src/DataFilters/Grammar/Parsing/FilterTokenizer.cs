using Superpower;
using Superpower.Model;
using System.Collections.Generic;
using static DataFilters.Grammar.Parsing.FilterToken;

namespace DataFilters.Grammar.Parsing
{
    public class FilterTokenizer : Tokenizer<FilterToken>
    {
        private const char _underscore = '_';
        private const char _asterisk = '*';
        private const char _equalSign = '=';

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

                            yield return Result.Value(Literal, identifierStart, next.Location);
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
                    case _underscore:
                        yield return Result.Value(Underscore, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case '|':
                        yield return Result.Value(Or, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case ',':
                        yield return Result.Value(And, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case _equalSign:
                        yield return Result.Value(Equal, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case _asterisk:
                        yield return Result.Value(Asterisk, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case '!':
                        yield return Result.Value(Not, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case '[':
                        yield return Result.Value(OpenSquaredBracket, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case ']':
                        yield return Result.Value(CloseSquaredBracket, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case '-':
                        yield return Result.Value(Dash, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case '(':
                        yield return Result.Value(OpenParenthese, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                    case ')':
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
                    default:
                        yield return Result.Value(None, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                        break;
                }

            } while (next.HasValue);
        }
    }
}

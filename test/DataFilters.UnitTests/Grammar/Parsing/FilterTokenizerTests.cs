namespace DataFilters.UnitTests.Grammar.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataFilters.Grammar.Parsing;
    using FluentAssertions;
    using Superpower;
    using Superpower.Model;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;
    using static DataFilters.Grammar.Parsing.FilterToken;

    [UnitTest]
    [Feature(nameof(DataFilters.Grammar.Parsing))]
    [Feature(nameof(FilterTokenizer))]
    public class FilterTokenizerTests
    {
        private readonly FilterTokenizer _sut;
        private readonly ITestOutputHelper _outputHelper;

        public FilterTokenizerTests(ITestOutputHelper outputHelper)
        {
            _sut = new FilterTokenizer();
            _outputHelper = outputHelper;
        }

        [Fact]
        public void IsTokenizer() => typeof(FilterTokenizer).Should()
                                                            .HaveDefaultConstructor().And
                                                            .BeAssignableTo<Tokenizer<FilterToken>>();

        public static IEnumerable<object[]> RecognizeTokensCases
        {
            get
            {
                yield return new object[]
                {
                    "Firstname=Bruce",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Exactly(15)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("F") && result.Span.Position.Column == 1)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("i") && result.Span.Position.Column == 2)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("r") && result.Span.Position.Column == 3)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("s") && result.Span.Position.Column == 4)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("t") && result.Span.Position.Column == 5)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("n") && result.Span.Position.Column == 6)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("a") && result.Span.Position.Column == 7)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("m") && result.Span.Position.Column == 8)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("e") && result.Span.Position.Column == 9)
                                                                                && results.Once(result => result.Kind == Equal && result.Span.EqualsValue("=")  && result.Span.Position.Column == 10)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("B") && result.Span.Position.Column == 11)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("r") && result.Span.Position.Column == 12)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("u") && result.Span.Position.Column == 13)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("c") && result.Span.Position.Column == 14)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("e") && result.Span.Position.Column == 15)
                    )
                };

                yield return new object[]
                {
                    "_Firstname=Bruce",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Exactly(16)
                                                                                && results.Once(result => result.Kind == Underscore && result.Span.EqualsValue("_") && result.Span.Position.Column == 1)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("F") && result.Span.Position.Column == 2)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("i") && result.Span.Position.Column == 3)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("r") && result.Span.Position.Column == 4)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("s") && result.Span.Position.Column == 5)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("t") && result.Span.Position.Column == 6)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("n") && result.Span.Position.Column == 7)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("a") && result.Span.Position.Column == 8)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("m") && result.Span.Position.Column == 9)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("e") && result.Span.Position.Column == 10)
                                                                                && results.Once(result => result.Kind == Equal && result.Span.EqualsValue("=")  && result.Span.Position.Column == 11)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("B") && result.Span.Position.Column == 12)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("r") && result.Span.Position.Column == 13)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("u") && result.Span.Position.Column == 14)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("c") && result.Span.Position.Column == 15)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("e") && result.Span.Position.Column == 16)
                    )
                };

                yield return new object[]
                {
                    "Firstname=Bru*",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Exactly(14)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("F") && result.Span.Position.Column == 1)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("i") && result.Span.Position.Column == 2)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("r") && result.Span.Position.Column == 3)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("s") && result.Span.Position.Column == 4)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("t") && result.Span.Position.Column == 5)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("n") && result.Span.Position.Column == 6)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("a") && result.Span.Position.Column == 7)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("m") && result.Span.Position.Column == 8)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("e") && result.Span.Position.Column == 9)
                                                                                && results.Once(result => result.Kind == Equal && result.Span.EqualsValue("=")  && result.Span.Position.Column == 10)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("B") && result.Span.Position.Column == 11)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("r") && result.Span.Position.Column == 12)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("u") && result.Span.Position.Column == 13)
                                                                                && results.Once(result => result.Kind == Asterisk && result.Span.EqualsValue("*") && result.Span.Position.Column == 14)
                    )
                };

                yield return new object[]
                {
                    "prop1=Bruce",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Exactly(11)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("p") && result.Span.Position.Column == 1)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("r") && result.Span.Position.Column == 2)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("o") && result.Span.Position.Column == 3)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("p") && result.Span.Position.Column == 4)
                                                                                && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("1") && result.Span.Position.Column == 5)
                                                                                && results.Once(result => result.Kind == Equal && result.Span.EqualsValue("=")  && result.Span.Position.Column == 6)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("B") && result.Span.Position.Column == 7)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("r") && result.Span.Position.Column == 8)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("u") && result.Span.Position.Column == 9)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("c") && result.Span.Position.Column == 10)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("e") && result.Span.Position.Column == 11)
                    )
                };

                yield return new object[]
                {
                    "val1|val2",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Exactly(9)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("v") && result.Span.Position.Column == 1)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("a") && result.Span.Position.Column == 2)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("l") && result.Span.Position.Column == 3)
                                                                                && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("1") && result.Span.Position.Column == 4)
                                                                                && results.Once(result => result.Kind == Or && result.Span.EqualsValue("|") && result.Span.Position.Column == 5)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("v")  && result.Span.Position.Column == 6)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("a") && result.Span.Position.Column == 7)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("l") && result.Span.Position.Column == 8)
                                                                                && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("2") && result.Span.Position.Column == 9)
                    )
                };

                yield return new object[]
                {
                    "val1,val2",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Exactly(9)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("v") && result.Span.Position.Column == 1)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("a") && result.Span.Position.Column == 2)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("l") && result.Span.Position.Column == 3)
                                                                                && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("1") && result.Span.Position.Column == 4)
                                                                                && results.Once(result => result.Kind == And && result.Span.EqualsValue(",") && result.Span.Position.Column == 5)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("v")  && result.Span.Position.Column == 6)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("a") && result.Span.Position.Column == 7)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("l") && result.Span.Position.Column == 8)
                                                                                && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("2") && result.Span.Position.Column == 9)
                    )
                };

                yield return new object[]
                {
                    "!Bruce",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Exactly(6)
                                                                                && results.Once(result => result.Kind == Bang && result.Span.EqualsValue("!") && result.Span.Position.Column == 1)
                                                                               && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("B") && result.Span.Position.Column == 2)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("r") && result.Span.Position.Column == 3)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("u") && result.Span.Position.Column == 4)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("c") && result.Span.Position.Column == 5)
                                                                                && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("e") && result.Span.Position.Column == 6)
                    )
                };

                yield return new object[]
                {
                    "[",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Once()
                                                                                && results.Once(result => result.Kind == OpenSquaredBracket && result.Span.EqualsValue("["))
                    )
                };

                yield return new object[]
                {
                    "]",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Once()
                                                                                && results.Once(result => result.Kind == CloseSquaredBracket && result.Span.EqualsValue("]"))
                    )
                };

                yield return new object[]
                {
                    "10-20",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Exactly(5)
                                                                                && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("1") && result.Position.Column == 1)
                                                                                && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("0") && result.Position.Column == 2)
                                                                                && results.Once(result => result.Kind == Dash  && result.Span.EqualsValue("-"))
                                                                                && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("2")  && result.Position.Column == 4)
                                                                                && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("0") && result.Position.Column == 5)
                    )
                };

                yield return new object[]
                {
                    " ",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Once()
                                                                                && results.Once(result => result.Kind == Whitespace && result.Span.EqualsValue(" "))
                    )
                };

                yield return new object[]
                {
                    ":",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(1)
                        && results.Once(result => result.Kind == Colon && result.Span.EqualsValue(":"))
                    )
                };

                yield return new object[]
                {
                    "2019-10-22",
                     (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(10)
                        && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("2") && result.Position.Column == 1)
                        && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("0") && result.Position.Column == 2)
                        && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("1") && result.Position.Column == 3)
                        && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("9") && result.Position.Column == 4)
                        && results.Once(result => result.Kind == Dash  && result.Span.EqualsValue("-") && result.Position.Column == 5)
                        && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("1") && result.Position.Column == 6)
                        && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("0") && result.Position.Column == 7)
                        && results.Once(result => result.Kind == Dash  && result.Span.EqualsValue("-") && result.Position.Column == 8)
                        && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("2") && result.Position.Column == 9)
                        && results.Once(result => result.Kind == Digit && result.Span.EqualsValue("2") && result.Position.Column == 10)
                    )
                };

                yield return new object[]
                {
                    "*",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(1)
                        && results.Exactly(result => result.Kind == Asterisk && result.Span.EqualsValue("*"), 1)
                    )
                };

                yield return new object[]
                {
                    "_a",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(2)
                        && results.Exactly(result => result.Kind == Underscore && result.Span.EqualsValue("_"), 1)
                        && results.Exactly(result => result.Kind == Letter && result.Span.EqualsValue("a"), 1)
                    )
                };

                foreach (char c in FilterTokenizer.SpecialCharacters)
                {
                    yield return new object[]
                    {
                        $"Firstname=Bru{FilterTokenizer.BackSlash}{c}",
                        (Expression<Func<TokenList<FilterToken>, bool>>) (results => results.Exactly(14)
                                                                                     && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("F") && result.Span.Position.Column == 1)
                                                                                     && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("i") && result.Span.Position.Column == 2)
                                                                                     && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("r") && result.Span.Position.Column == 3)
                                                                                     && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("s") && result.Span.Position.Column == 4)
                                                                                     && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("t") && result.Span.Position.Column == 5)
                                                                                     && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("n") && result.Span.Position.Column == 6)
                                                                                     && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("a") && result.Span.Position.Column == 7)
                                                                                     && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("m") && result.Span.Position.Column == 8)
                                                                                     && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("e") && result.Span.Position.Column == 9)
                                                                                     && results.Once(result => result.Kind == Equal && result.Span.EqualsValue("=")  && result.Span.Position.Column == 10)
                                                                                     && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("B") && result.Span.Position.Column == 11)
                                                                                     && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("r") && result.Span.Position.Column == 12)
                                                                                     && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("u") && result.Span.Position.Column == 13)
                                                                                     && results.Once(result => result.Kind == Escaped && result.Span.EqualsValue(c.ToString()) && result.Span.Position.Column == 15)
                        )
                    };
                }

                yield return new object[]
                {
                    @"\",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(1)
                        && results.Once(result => result.Kind == Letter && result.Span.EqualsValue(@"\"))
                    )
                };

                yield return new object[]
                {
                    @"\\t",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(2)
                        && results.Once(result => result.Kind == Escaped && result.Span.EqualsValue("\\"))
                        && results.Once(result => result.Kind == Letter && result.Span.EqualsValue("t"))
                    )
                };

                yield return new object[]
                {
                    @"""",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(1)
                        && results.Once(result => result.Kind == DoubleQuote && result.Span.EqualsValue(@""""))
                    )
                };

                yield return new object[]
                {
                    "+",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Once()
                                                                                && results.Once(result => result.Kind == None && result.Span.EqualsValue("+"))
                    )
                };

                foreach (char chr in FilterTokenizer.SpecialCharacters)
                {
                    switch (chr)
                    {
                        case FilterTokenizer.DoubleQuote:
                            yield return new object[]
                            {
                                @$"""\{chr}""",
                                (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Exactly(3)
                                                                                            && results.Once(result => result.Kind == DoubleQuote
                                                                                                                         && result.Position.Column == 1)
                                                                                            && results.Once(result => result.Kind == Escaped
                                                                                                                      && result.Position.Column == 3
                                                                                                                      && result.Span.EqualsValue(@""""))
                                                                                            && results.Once(result => result.Kind == DoubleQuote
                                                                                                                      && result.Position.Column == 4)
                                )
                            };
                            break;
                        case FilterTokenizer.BackSlash:
                            yield return new object[]
                            {
                                @$"""\{chr}""",
                                (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Exactly(3)
                                                                                            && results.Once(result => result.Kind == DoubleQuote && result.Position.Column == 1)
                                                                                            && results.Once(result => result.Kind == Escaped
                                                                                                                      && result.Position.Column == 3
                                                                                                                      && result.Span.EqualsValue(@"\"))
                                                                                            && results.Once(result => result.Kind == DoubleQuote
                                                                                                                      && result.Position.Column == 4)
                                )
                            };
                            break;
                        default:
                            yield return new object[]
                            {
                                @$"""\{chr}""",
                                (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Exactly(4)
                                                                                            && results.Once(result => result.Kind == DoubleQuote && result.Position.Column == 1)
                                                                                            && results.Once(result => result.Kind == Escaped && result.Position.Column == 2 && result.Span.EqualsValue("\\"))
                                                                                            && results.Once(result => result.Kind == Escaped && result.Position.Column == 3 && result.Span.EqualsValue($"{chr}"))
                                                                                            && results.Once(result => result.Kind == DoubleQuote && result.Position.Column == 4)
                                )
                            };
                            break;
                    }
                }

                yield return new object[]
                {
                    @"""\[",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results => results.Exactly(3)
                                                                                && results.Once(result => result.Kind == DoubleQuote && result.Span.EqualsValue(@""""))
                                                                                && results.Once(result => result.Kind == Escaped && result.Span.EqualsValue(@"\"))
                                                                                && results.Once(result => result.Kind == Escaped && result.Span.EqualsValue("["))
                    )
                };
            }
        }

        [Theory]
        [MemberData(nameof(RecognizeTokensCases))]
        public void RecognizeTokens(string input, Expression<Func<TokenList<FilterToken>, bool>> expectation)
        {
            _outputHelper.WriteLine($"input : '{input}'");
            // Act
            TokenList<FilterToken> tokens = _sut.Tokenize(input);

            // Assert 
            _outputHelper.WriteLine($"Tokens : {tokens.Select(token => new { Value = token.ToStringValue(), Kind = token.Kind.ToString(), token.Position.Column }).Jsonify()}");

            tokens.Should()
                .Match(expectation);
        }
    }
}
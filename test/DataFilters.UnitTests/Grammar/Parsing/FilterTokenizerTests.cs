using DataFilters.Grammar.Parsing;
using FluentAssertions;
using Superpower;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static DataFilters.Grammar.Parsing.FilterToken;

namespace DataFilters.UnitTests.Grammar.Parsing
{
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

        public static IEnumerable<object[]> RecognizeTokensCases
        {
            get
            {
                yield return new object[]
                {
                    "Firstname=Bruce",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(3)
                        && results.Once(result => result.Kind == Alpha && result.Span.EqualsValue("Firstname"))
                        && results.Once(result => result.Kind == Equal && result.Span.EqualsValue("="))
                        && results.Once(result => result.Kind == Alpha && result.Span.EqualsValue("Bruce"))
                    )
                };

                yield return new object[]
                {
                    "_Firstname=Bruce",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(4)
                        && results.Once(result => result.Kind == Underscore && result.Span.EqualsValue("_"))
                        && results.Once(result => result.Kind == Alpha && result.Span.EqualsValue("Firstname"))
                        && results.Once(result => result.Kind == Equal && result.Span.EqualsValue("="))
                        && results.Once(result => result.Kind == Alpha && result.Span.EqualsValue("Bruce"))
                    )
                };

                yield return new object[]
                {
                    "Firstname=Bru*",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(4)
                        && results.Once(result => result.Kind == Alpha && result.Span.EqualsValue("Firstname"))
                        && results.Once(result => result.Kind == Equal && result.Span.EqualsValue("="))
                        && results.Once(result => result.Kind == Alpha && result.Span.EqualsValue("Bru"))
                        && results.Once(result => result.Kind == Asterisk && result.Span.EqualsValue("*"))
                    )
                };

                yield return new object[]
                {
                    "prop1=Bruce",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(4)
                        && results.Once(result => result.Kind == Alpha && result.Span.EqualsValue("prop"))
                        && results.Once(result => result.Kind == Numeric && result.Span.EqualsValue("1"))
                        && results.Once(result => result.Kind == Equal && result.Span.EqualsValue("="))
                        && results.Once(result => result.Kind == Alpha && result.Span.EqualsValue("Bruce"))
                    )
                };

                yield return new object[]
                {
                    "val1|val2",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(5)
                        && results.Exactly(result => result.Kind == Alpha && result.Span.EqualsValue("val"), 2)
                        && results.Once(result => result.Kind == Numeric && result.Span.EqualsValue("1"))
                        && results.Once(result => result.Kind == Or && result.Span.EqualsValue("|"))
                        && results.Once(result => result.Kind == Numeric && result.Span.EqualsValue("2"))
                    )
                };

                yield return new object[]
                {
                    "val1,val2",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(5)
                        && results.Exactly(result => result.Kind == Alpha && result.Span.EqualsValue("val"), 2)
                        && results.Once(result => result.Kind == Numeric && result.Span.EqualsValue("1"))
                        && results.Once(result => result.Kind == And && result.Span.EqualsValue(","))
                        && results.Once(result => result.Kind == Numeric && result.Span.EqualsValue("2"))
                    )
                };

                yield return new object[]
                {
                    "!Bruce",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(2)
                        && results.Once(result => result.Kind == Not && result.Span.EqualsValue("!"))
                        && results.Once(result => result.Kind == Alpha && result.Span.EqualsValue("Bruce"))
                    )
                };

                yield return new object[]
                {
                    "!Bruce",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(2)
                        && results.Once(result => result.Kind == Not && result.Span.EqualsValue("!"))
                        && results.Once(result => result.Kind == Alpha && result.Span.EqualsValue("Bruce"))
                    )
                };

                yield return new object[]
                {
                    "[",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(1)
                        && results.Once(result => result.Kind == OpenSquaredBracket && result.Span.EqualsValue("["))
                    )
                };

                yield return new object[]
                {
                    "]",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(1)
                        && results.Once(result => result.Kind == CloseSquaredBracket && result.Span.EqualsValue("]"))
                    )
                };

                yield return new object[]
                {
                    "10-20",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(3)
                        && results.Once(result => result.Kind == Numeric && result.Span.EqualsValue("10"))
                        && results.Once(result => result.Kind == Dash && result.Span.EqualsValue("-"))
                        && results.Once(result => result.Kind == Numeric && result.Span.EqualsValue("20"))
                    )
                };

                yield return new object[]
                {
                    " ",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(1)
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
                        results.Exactly(5)
                        && results.Exactly(result => result.Kind == Dash && result.Span.EqualsValue("-"), 2)
                        && results.Once(result => result.Kind == Numeric && result.Span.EqualsValue("2019"))
                        && results.Once(result => result.Kind == Numeric && result.Span.EqualsValue("10"))
                        && results.Once(result => result.Kind == Numeric && result.Span.EqualsValue("22"))
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
                        && results.Exactly(result => result.Kind == Alpha && result.Span.EqualsValue("a"), 1)
                    )
                };

                foreach (char c in FilterTokenizer.SpecialCharacters)
                {
                    yield return new object[]
                    {
                        $"Firstname=Bru{FilterTokenizer.BackSlash}{c}",
                        (Expression<Func<TokenList<FilterToken>, bool>>) (results =>
                            results.Exactly(4)
                            && results.Once(result =>
                                result.Kind == Alpha && result.Span.EqualsValue("Firstname"))
                            && results.Once(result => result.Kind == Equal && result.Span.EqualsValue("="))
                            && results.Once(
                                result => result.Kind == Alpha && result.Span.EqualsValue("Bru"))
                            && results.Once(result =>
                                result.Kind == Escaped && result.Span.EqualsValue(c.ToString()))
                        )

                    };
                }

                yield return new object[]
                {
                    @"\",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(1)
                        && results.Exactly(result => result.Kind == Alpha && result.Span.EqualsValue(@"\"), 1)
                    )
                };
                yield return new object[]
                {
                    @"\\t",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(2)
                        && results.Exactly(result => result.Kind == Escaped && result.Span.EqualsValue("\\"), 1)
                        && results.Exactly(result => result.Kind == Alpha && result.Span.EqualsValue("t"), 1)
                    )
                };
            }
        }

        [Fact]
        public void IsTokenizer() => typeof(FilterTokenizer).Should()
            .HaveDefaultConstructor().And
            .BeAssignableTo<Tokenizer<FilterToken>>();

        [Theory]
        [MemberData(nameof(RecognizeTokensCases))]
        public void RecognizeTokens(string input, Expression<Func<TokenList<FilterToken>, bool>> expectation)
        {
            // Act
            TokenList<FilterToken> tokens = _sut.Tokenize(input);

            // Assert
            _outputHelper.WriteLine($"Tokens : {tokens.Select(token => new { Value = token.ToStringValue(), token.Kind, token.Position.Column, token.Position.Line, }).Jsonify()}");

            tokens.Should()
                .Match(expectation);
        }
    }
}
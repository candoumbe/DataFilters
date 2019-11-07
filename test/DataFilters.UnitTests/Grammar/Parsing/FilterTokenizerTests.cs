using DataFilters.Grammar.Parsing;
using FluentAssertions;
using Superpower;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

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
                        && results.Once(result => result.Kind == FilterToken.Literal && result.Span.EqualsValue("Firstname"))
                        && results.Once(result => result.Kind == FilterToken.Equal && result.Span.EqualsValue("="))
                        && results.Once(result => result.Kind == FilterToken.Literal && result.Span.EqualsValue("Bruce"))
                    )
                };

                yield return new object[]
                {
                    "_Firstname=Bruce",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(4)
                        && results.Once(result => result.Kind == FilterToken.Underscore && result.Span.EqualsValue("_"))
                        && results.Once(result => result.Kind == FilterToken.Literal && result.Span.EqualsValue("Firstname"))
                        && results.Once(result => result.Kind == FilterToken.Equal && result.Span.EqualsValue("="))
                        && results.Once(result => result.Kind == FilterToken.Literal && result.Span.EqualsValue("Bruce"))
                    )
                };

                yield return new object[]
                {
                    "Firstname=Bru*",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(4)
                        && results.Once(result => result.Kind == FilterToken.Literal && result.Span.EqualsValue("Firstname"))
                        && results.Once(result => result.Kind == FilterToken.Equal && result.Span.EqualsValue("="))
                        && results.Once(result => result.Kind == FilterToken.Literal && result.Span.EqualsValue("Bru"))
                        && results.Once(result => result.Kind == FilterToken.Asterisk && result.Span.EqualsValue("*"))
                    )
                };

                yield return new object[]
                {
                    "prop1=Bruce",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(4)
                        && results.Once(result => result.Kind == FilterToken.Literal && result.Span.EqualsValue("prop"))
                        && results.Once(result => result.Kind == FilterToken.Numeric && result.Span.EqualsValue("1"))
                        && results.Once(result => result.Kind == FilterToken.Equal && result.Span.EqualsValue("="))
                        && results.Once(result => result.Kind == FilterToken.Literal && result.Span.EqualsValue("Bruce"))
                    )
                };

                yield return new object[]
                {
                    "val1|val2",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(5)
                        && results.Exactly(result => result.Kind == FilterToken.Literal && result.Span.EqualsValue("val"), 2)
                        && results.Once(result => result.Kind == FilterToken.Numeric && result.Span.EqualsValue("1"))
                        && results.Once(result => result.Kind == FilterToken.Or && result.Span.EqualsValue("|"))
                        && results.Once(result => result.Kind == FilterToken.Numeric && result.Span.EqualsValue("2"))
                    )
                };

                yield return new object[]
                {
                    "val1,val2",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(5)
                        && results.Exactly(result => result.Kind == FilterToken.Literal && result.Span.EqualsValue("val"), 2)
                        && results.Once(result => result.Kind == FilterToken.Numeric && result.Span.EqualsValue("1"))
                        && results.Once(result => result.Kind == FilterToken.And && result.Span.EqualsValue(","))
                        && results.Once(result => result.Kind == FilterToken.Numeric && result.Span.EqualsValue("2"))
                    )
                };

                yield return new object[]
                {
                    "!Bruce",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(2)
                        && results.Once(result => result.Kind == FilterToken.Not && result.Span.EqualsValue("!"))
                        && results.Once(result => result.Kind == FilterToken.Literal && result.Span.EqualsValue("Bruce"))
                    )
                };

                yield return new object[]
                {
                    "!Bruce",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(2)
                        && results.Once(result => result.Kind == FilterToken.Not && result.Span.EqualsValue("!"))
                        && results.Once(result => result.Kind == FilterToken.Literal && result.Span.EqualsValue("Bruce"))
                    )
                };

                yield return new object[]
                {
                    "[",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(1)
                        && results.Once(result => result.Kind == FilterToken.OpenSquaredBracket && result.Span.EqualsValue("["))
                    )
                };

                yield return new object[]
                {
                    "]",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(1)
                        && results.Once(result => result.Kind == FilterToken.CloseSquaredBracket && result.Span.EqualsValue("]"))
                    )
                };

                yield return new object[]
                {
                    "10-20",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(3)
                        && results.Once(result => result.Kind == FilterToken.Numeric && result.Span.EqualsValue("10"))
                        && results.Once(result => result.Kind == FilterToken.Dash && result.Span.EqualsValue("-"))
                        && results.Once(result => result.Kind == FilterToken.Numeric && result.Span.EqualsValue("20"))
                    )
                };

                yield return new object[]
                {
                    " ",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(1)
                        && results.Once(result => result.Kind == FilterToken.Whitespace && result.Span.EqualsValue(" "))
                    )
                };

                yield return new object[]
                {
                    ":",
                    (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(1)
                        && results.Once(result => result.Kind == FilterToken.Colon && result.Span.EqualsValue(":"))
                    )
                };

                yield return new object[]
                {
                    "2019-10-22",
                     (Expression<Func<TokenList<FilterToken>, bool>>)(results =>
                        results.Exactly(5)
                        && results.Exactly(result => result.Kind == FilterToken.Dash && result.Span.EqualsValue("-"), 2)
                        && results.Once(result => result.Kind == FilterToken.Numeric && result.Span.EqualsValue("2019"))
                        && results.Once(result => result.Kind == FilterToken.Numeric && result.Span.EqualsValue("10"))
                        && results.Once(result => result.Kind == FilterToken.Numeric && result.Span.EqualsValue("22"))
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
            _outputHelper.WriteLine($"Tokens : {tokens.Jsonify()}");

            tokens.Should()
                .Match(expectation);
        }
    }
}
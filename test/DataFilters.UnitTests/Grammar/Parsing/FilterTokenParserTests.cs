using DataFilters.Grammar.Parsing;
using DataFilters.Grammar.Syntax;
using FluentAssertions;
using FluentAssertions.Execution;
using Superpower;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using ConstantExpression = DataFilters.Grammar.Syntax.ConstantExpression;

namespace DataFilters.UnitTests.Grammar.Parsing
{
    /// <summary>
    /// Unit tests for  <see cref="FilterTokenParser"/>
    /// </summary>
    [UnitTest]
    [Feature(nameof(DataFilters.Grammar.Parsing))]
    [Feature(nameof(FilterTokenParser))]
    public class FilterTokenParserTests
    {
        private readonly FilterTokenizer _tokenizer;
        private readonly ITestOutputHelper _outputHelper;

        public FilterTokenParserTests(ITestOutputHelper outputHelper)
        {
            _tokenizer = new FilterTokenizer();
            _outputHelper = outputHelper;
        }

        [Fact]
        public void IsParser() => typeof(FilterTokenParser).Should()
            .BeStatic().And
            .HaveProperty<TokenListParser<FilterToken, ConstantExpression>>("Constant").And
            .HaveProperty<TokenListParser<FilterToken, StartsWithExpression>>("StartsWith").And
            .HaveProperty<TokenListParser<FilterToken, EndsWithExpression>>("EndsWith").And
            .HaveProperty<TokenListParser<FilterToken, OneOfExpression>>("OneOf").And
            .HaveProperty<TokenListParser<FilterToken, ContainsExpression>>("Contains").And
            .HaveProperty<TokenListParser<FilterToken, AsteriskExpression>>("Asterisk").And
            .HaveProperty<TokenListParser<FilterToken, GroupExpression>>("Group").And
            .HaveProperty<TokenListParser<FilterToken, AndExpression>>("And").And
            .HaveProperty<TokenListParser<FilterToken, OrExpression>>("Or");

        [Fact]
        public void CanParseConstant()
        {
            // Arrange
            const string input = "Bruce";
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);
            _outputHelper.WriteLine($"Tokens : ${tokens.Jsonify()}");

            // Act
            FilterExpression expression = FilterTokenParser.Constant.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, new ConstantExpression("Bruce"));
        }

        [Fact]
        public void CanParseStartsWith()
        {
            // Arrange
            const string input = "Bru*";
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);
            _outputHelper.WriteLine($"Tokens : ${tokens.Jsonify()}");

            // Act
            StartsWithExpression expression = FilterTokenParser.StartsWith.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, new StartsWithExpression("Bru"));
        }

        public static IEnumerable<object[]> EndsWithCases
        {
            get
            {
                yield return new object[]
                {
                    "*ce",
                    new EndsWithExpression("ce")
                };
            }
        }

        [Theory]
        [MemberData(nameof(EndsWithCases))]
        public void CanParseEndsWith(string input, EndsWithExpression expected)
        {
            // Arrange
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);
            _outputHelper.WriteLine($"Tokens : ${tokens.Jsonify()}");

            // Act
            EndsWithExpression expression = FilterTokenParser.EndsWith.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, expected);
        }

        [Fact]
        public void CanParseContains()
        {
            // Arrange
            const string input = "*bat*";
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            ContainsExpression expression = FilterTokenParser.Contains.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, new ContainsExpression("bat"));
        }

        public static IEnumerable<object[]> OrExpressionCases
        {
            get
            {
                yield return new object[]
                {
                    "Bruce|bat*",
                    new OrExpression(new ConstantExpression("Bruce"), new StartsWithExpression("bat"))
                };

                yield return new object[]
                {
                    "Bruce|Wayne",
                    new OrExpression(new ConstantExpression("Bruce"), new ConstantExpression("Wayne"))
                };

                yield return new object[]
                {
                    "Bru*|Di*",
                    new OrExpression(new StartsWithExpression("Bru"), new StartsWithExpression("Di"))
                };

                yield return new object[]
                {
                    "!(Bat*|Sup*)|!*man",
                    new OrExpression(
                        left : new NotExpression(
                                new GroupExpression(
                                    new OrExpression(new StartsWithExpression("Bat"), new StartsWithExpression("Sup"))
                                )
                            ),
                        right: new NotExpression(new EndsWithExpression("man"))
                    )
                };
            }
        }

        [Theory]
        [MemberData(nameof(OrExpressionCases))]
        public void CanParseOrExpression(string input, OrExpression expected)
        {
            // Arrange
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            OrExpression expression = FilterTokenParser.Or.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, expected);
        }

        public static IEnumerable<object[]> AndExpressionCases
        {
            get
            {
                yield return new object[]
                {
                    "bat*,man*",
                    new AndExpression(new StartsWithExpression("bat"), new StartsWithExpression("man"))
                };

                yield return new object[]
                {
                    "!(Bat*|Sup*),!*man",
                    new AndExpression(
                        left : new NotExpression(
                                new GroupExpression(
                                    new OrExpression(new StartsWithExpression("Bat"), new StartsWithExpression("Sup"))
                                )
                            ),
                        right: new NotExpression(new EndsWithExpression("man"))
                    )
                };

                yield return new object[]
                {
                    "bat*man",
                    new AndExpression(new StartsWithExpression("bat"), new EndsWithExpression("man"))
                };
            }
        }

        [Theory]
        [MemberData(nameof(AndExpressionCases))]
        public void CanParseAndExpression(string input, AndExpression expected)
        {
            // Arrange
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            AndExpression expression = FilterTokenParser.And.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, expected);
        }

        public static IEnumerable<object[]> NotExpressionCases
        {
            get
            {
                yield return new object[]
                {
                    "!Bruce",
                    new NotExpression(new ConstantExpression("Bruce"))
                };

                yield return new object[]
                {
                    "!!Bruce",
                    new NotExpression(new NotExpression(new ConstantExpression("Bruce")))
                };

                yield return new object[]
                {
                    "!Bru*",
                    new NotExpression(new StartsWithExpression("Bru"))
                };

                yield return new object[]
                {
                    "![10 TO *]",
                    new NotExpression(new RangeExpression(min : new ConstantExpression("10")))
                };

                yield return new object[]
                {
                    "!(B*|C*)",
                    new NotExpression(new GroupExpression(new OrExpression(new StartsWithExpression("B"), new StartsWithExpression("C"))))
                };
            }
        }

        [Theory]
        [MemberData(nameof(NotExpressionCases))]
        public void CanParseNotExpression(string input, NotExpression expected)
        {
            // Arrange
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            NotExpression expression = FilterTokenParser.Not.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, expected);
        }

        public static IEnumerable<object[]> OneOfExpressionCases
        {
            get
            {
                yield return new object[]
                {
                    "[Bb]",
                    new OneOfExpression(new ConstantExpression("B"),new ConstantExpression("b"))
                };

                yield return new object[]
                {
                    "[Bb]ruce",
                    new OneOfExpression(new ConstantExpression("Bruce"), new ConstantExpression("bruce"))
                };

                yield return new object[]
                {
                    "[Bb]at*",
                    new OneOfExpression(new StartsWithExpression("Bat"), new StartsWithExpression("bat"))
                };

                yield return new object[]
                {
                    "Ma*[Nn]",
                    new OneOfExpression(
                        new AndExpression(new StartsWithExpression("Ma"), new EndsWithExpression("N")),
                        new AndExpression(new StartsWithExpression("Ma"), new EndsWithExpression("n"))
                    )
                };

                yield return new object[]
                {
                    "cat[Ww]oman",
                    new OneOfExpression(new ConstantExpression("catWoman"), new ConstantExpression("catwoman"))
                };

                yield return new object[]
                {
                    "*Br[Uu]",
                    new OneOfExpression(new EndsWithExpression("BrU"), new EndsWithExpression("Bru"))
                };

                yield return new object[]
                {
                    "Bo[Bb]",
                    new OneOfExpression(new ConstantExpression("BoB"), new ConstantExpression("Bob"))
                };
            }
        }

        [Theory]
        [MemberData(nameof(OneOfExpressionCases))]
        public void CanParseOneOfExpression(string input, OneOfExpression expected)
        {
            // Arrange
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            OneOfExpression expression = FilterTokenParser.OneOf.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, expected);
        }

        public static IEnumerable<object[]> RangeCases
        {
            get
            {
                yield return new object[]
                {
                    "[10 TO 20]",
                    new RangeExpression(min : new ConstantExpression("10"), max: new ConstantExpression("20"))
                };

                yield return new object[]
                {
                    "[* TO 20]",
                    new RangeExpression(max: new ConstantExpression("20"))
                };

                yield return new object[]
                {
                    "[10 TO *]",
                    new RangeExpression(min: new ConstantExpression("10"))
                };

                yield return new object[]
                {
                    "[2010-06-25 TO 2010-06-29]",
                    new RangeExpression(min: new ConstantExpression("2010-06-25"), max: new ConstantExpression("2010-06-29"))
                };
            }
        }

        [Theory]
        [MemberData(nameof(RangeCases))]
        public void CanParseRange(string input, RangeExpression expected)
        {
            // Arrange
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            RangeExpression expression = FilterTokenParser.Range.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, expected);
        }

        public static IEnumerable<object[]> GroupCases
        {
            get
            {
                yield return new object[]
                {
                    "(Bruce*)",
                    new GroupExpression(new StartsWithExpression("Bruce"))
                };

                yield return new object[]
                {
                    "(Bru*|Di*)",
                    new GroupExpression(new OrExpression(new StartsWithExpression("Bru"), new StartsWithExpression("Di")))
                };

                yield return new object[]
                {
                    "(B*,*man)",
                    new GroupExpression(new AndExpression(new StartsWithExpression("B"), new EndsWithExpression("man")))
                };
            }
        }

        [Theory]
        [MemberData(nameof(GroupCases))]
        public void CanParseGroup(string input, GroupExpression expected)
        {
            // Arrange
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            GroupExpression expression = FilterTokenParser.Group.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, expected);
        }

        public static IEnumerable<object[]> PropertyNameCases
        {
            get
            {
                yield return new object[]
                {
                    "Firstname=Bruce",
                    new PropertyNameExpression("Firstname")
                };
            }
        }

        [Theory]
        [MemberData(nameof(PropertyNameCases))]
        public void CanParsePropertyNameExpression(string input, PropertyNameExpression expected)
        {
            // Arrange
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            PropertyNameExpression expression = FilterTokenParser.Property.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, expected);
        }

        public static IEnumerable<object[]> CriterionCases
        {
            get
            {
                yield return new object[]
                {
                    "Name=Vandal",
                    (
                        new PropertyNameExpression("Name"),
                        (FilterExpression) new ConstantExpression("Vandal")
                    )
                };

                yield return new object[]
                {
                    "Name=Vandal|Banner",
                    (
                        new PropertyNameExpression("Name"),
                        (FilterExpression) new OrExpression(new ConstantExpression("Vandal"), new ConstantExpression("Banner"))
                    )
                };
            }
        }

        /// <summary>
        /// Tests if <see cref="FilterTokenParser.Criteria"/> can parse a criteria
        /// </summary>
        /// <param name="input"></param>
        /// <param name="expected"></param>
        [Theory]
        [MemberData(nameof(CriterionCases))]
        public void CanParseCriterion(string input, (PropertyNameExpression prop, FilterExpression expression) expected)
        {
            // Arrange
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            (PropertyNameExpression prop, FilterExpression expression) = FilterTokenParser.Criterion.Parse(tokens);

            // Assert
            using (new AssertionScope())
            {
                prop.Should()
                    .NotBeSameAs(expected.prop).And
                    .Be(expected.prop);

                expression.Should()
                    .NotBeSameAs(expected.expression).And
                    .Be(expected.expression);
            }
        }

        public static IEnumerable<object[]> CriteriaCases
        {
            get
            {
                yield return new object[]
                {
                    "Firstname=Vandal&Lastname=Savage",
                    (Expression<Func<IEnumerable<(PropertyNameExpression prop, FilterExpression expression)>, bool>>)(
                        expressions => expressions.Exactly(2)
                        && expressions.Once(expr => expr.prop.Equals(new PropertyNameExpression("Firstname")) && expr.expression.Equals(new ConstantExpression("Vandal")))
                        && expressions.Once(expr => expr.prop.Equals(new PropertyNameExpression("Lastname")) && expr.expression.Equals(new ConstantExpression("Savage")))
                    )
                };

                yield return new object[]
                {
                    "Firstname=[Vv]andal",
                    (Expression<Func<IEnumerable<(PropertyNameExpression prop, FilterExpression expression)>, bool>>)(
                        expressions => expressions.Exactly(1)
                        && expressions.Once(expr => expr.prop.Equals(new PropertyNameExpression("Firstname"))
                            && expr.expression.Equals(new OneOfExpression(new ConstantExpression("Vandal"), new ConstantExpression("vandal"))))
                    )
                };
            }
        }

        /// <summary>
        /// Tests if <see cref="FilterTokenParser.Criteria"/> can parse a criteria
        /// </summary>
        /// <param name="input"></param>
        /// <param name="expected"></param>
        [Theory]
        [MemberData(nameof(CriteriaCases))]
        public void CanParseCriteria(string input, Expression<Func<IEnumerable<(PropertyNameExpression prop, FilterExpression expression)>, bool>> expectation)
        {
            // Arrange
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            IEnumerable<(PropertyNameExpression prop, FilterExpression expression)> actual = FilterTokenParser.Criteria.Parse(tokens);

            // Assert
            actual.Should()
                .Match(expectation);
        }

        private static void AssertThatCanParse(FilterExpression expression, FilterExpression expected)
            => expression.Should()
                .NotBeSameAs(expected).And
                .Be(expected);
    }
}

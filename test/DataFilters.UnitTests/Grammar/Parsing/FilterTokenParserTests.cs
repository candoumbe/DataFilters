using DataFilters.Grammar.Parsing;
using DataFilters.Grammar.Syntax;
using FluentAssertions;
using FluentAssertions.Execution;
using Superpower;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

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
                                                           .HaveProperty<TokenListParser<FilterToken, ConstantValueExpression>>("AlphaNumeric").And
                                                           .HaveProperty<TokenListParser<FilterToken, PropertyNameExpression>>("Property").And
                                                           .HaveProperty<TokenListParser<FilterToken, StartsWithExpression>>("StartsWith").And
                                                           .HaveProperty<TokenListParser<FilterToken, EndsWithExpression>>("EndsWith").And
                                                           .HaveProperty<TokenListParser<FilterToken, OneOfExpression>>("OneOf").And
                                                           .HaveProperty<TokenListParser<FilterToken, ContainsExpression>>("Contains").And
                                                           .HaveProperty<TokenListParser<FilterToken, AsteriskExpression>>("Asterisk").And
                                                           .HaveProperty<TokenListParser<FilterToken, GroupExpression>>("Group").And
                                                           .HaveProperty<TokenListParser<FilterToken, AndExpression>>("And").And
                                                           .HaveProperty<TokenListParser<FilterToken, RangeExpression>>("Range").And
                                                           .HaveProperty<TokenListParser<FilterToken, OrExpression>>("Or");

        public static IEnumerable<object[]> AlphaNumericCases
        {
            get
            {
                yield return new object[]
                {
                    "Bruce",
                    new ConstantValueExpression("Bruce")
                };

                yield return new object[]
                {
                    @"Vandal\*",
                    new ConstantValueExpression("Vandal*")
                };

                yield return new object[]
                {
                    @"Van\*dal",
                    new ConstantValueExpression("Van*dal")
                };
                yield return new object[]
                {
                    "1Bruce",
                    new ConstantValueExpression("1Bruce")
                };
            }
        }

        [Theory]
        [MemberData(nameof(AlphaNumericCases))]
        public void CanParseAlphaNumeric(string input, ConstantValueExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);
            _outputHelper.WriteLine($"Tokens : ${tokens.Jsonify()}");

            // Act
            FilterExpression expression = FilterTokenParser.AlphaNumeric.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, expected);
        }

        public static IEnumerable<object[]> StartsWithCases
        {
            get
            {
                yield return new object[]
                {
                    "ce*",
                    new StartsWithExpression("ce")
                };

                string[] punctuations = { ".", "-", ":", "_" };

                foreach (string punctuation in punctuations)
                {
                    yield return new object[]
                    {
                        $"{punctuation}ce*",
                        new StartsWithExpression($"{punctuation}ce")
                    };

                    yield return new object[]
                    {
                        $"ce{punctuation}*",
                        new StartsWithExpression($"ce{punctuation}")
                    };

                    yield return new object[]
                    {
                        $"c{punctuation}e*",
                        new StartsWithExpression($"c{punctuation}e")
                    };

                    yield return new object[]
                    {
                        $"{punctuation}*",
                        new StartsWithExpression(punctuation)
                    };

                    yield return new object[]
                    {
                        $"{punctuation}{punctuation}ce*",
                        new StartsWithExpression($"{punctuation}{punctuation}ce")
                    };

                    yield return new object[]
                    {
                        $"{punctuation}{punctuation}ce{punctuation}{punctuation}*",
                        new StartsWithExpression($"{punctuation}{punctuation}ce{punctuation}{punctuation}")
                    };

                    yield return new object[]
                    {
                        $"c{punctuation}e{punctuation}*",
                        new StartsWithExpression($"c{punctuation}e{punctuation}")
                    };

                    yield return new object[]
                    {
                        $"{punctuation}c{punctuation}e*",
                        new StartsWithExpression($"{punctuation}c{punctuation}e")
                    };

                    yield return new object[]
                    {
                        $@"\*{punctuation}c{punctuation}e*",
                        new StartsWithExpression($"*{punctuation}c{punctuation}e")
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(StartsWithCases))]
        public void CanParseStartsWith(string input, StartsWithExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);
            _outputHelper.WriteLine($"Tokens : ${tokens.Select(token => new { token.Kind, token.Position.Line, token.Position.Column }).Jsonify()}");

            // Act
            StartsWithExpression expression = FilterTokenParser.StartsWith.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, expected);
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

                string[] punctuations = { ".", "-", ":", "_" };

                foreach (string punctuation in punctuations)
                {
                    yield return new object[]
                    {
                        $"*{punctuation}ce",
                        new EndsWithExpression($"{punctuation}ce")
                    };

                    yield return new object[]
                    {
                        $"*ce{punctuation}",
                        new EndsWithExpression($"ce{punctuation}")
                    };

                    yield return new object[]
                    {
                        $"*c{punctuation}e",
                        new EndsWithExpression($"c{punctuation}e")
                    };

                    yield return new object[]
                    {
                        $"*{punctuation}",
                        new EndsWithExpression(punctuation)
                    };

                    yield return new object[]
                    {
                        $"*{punctuation}{punctuation}ce",
                        new EndsWithExpression($"{punctuation}{punctuation}ce")
                    };

                    yield return new object[]
                    {
                        $"*{punctuation}{punctuation}ce{punctuation}{punctuation}",
                        new EndsWithExpression($"{punctuation}{punctuation}ce{punctuation}{punctuation}")
                    };

                    yield return new object[]
                    {
                        $"*c{punctuation}e{punctuation}",
                        new EndsWithExpression($"c{punctuation}e{punctuation}")
                    };

                    yield return new object[]
                    {
                        $"*{punctuation}c{punctuation}e",
                        new EndsWithExpression($"{punctuation}c{punctuation}e")
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EndsWithCases))]
        public void CanParseEndsWith(string input, EndsWithExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);
            _outputHelper.WriteLine($"Tokens : ${tokens.Jsonify()}");

            // Act
            EndsWithExpression expression = FilterTokenParser.EndsWith.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, expected);
        }

        public static IEnumerable<object[]> ContainsCases
        {
            get
            {
                yield return new object[]
                {
                    "*bat*",
                    new ContainsExpression("bat")
                };

                yield return new object[]
                {
                    @"*bat\*man*",
                    new ContainsExpression("bat*man")
                };

                yield return new object[]
                {
                    "*d3aa022d-ec52-47aa-be13-6823c478c60a*",
                    new ContainsExpression("d3aa022d-ec52-47aa-be13-6823c478c60a")
                };

                string[] punctuations = { ".", "-", ":", "_" };

                foreach (string punctuation in punctuations)
                {
                    yield return new object[]
                    {
                        $"*{punctuation}*",
                        new ContainsExpression(punctuation)
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ContainsCases))]
        public void CanParseContains(string input, ContainsExpression expectedContains)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            ContainsExpression expression = FilterTokenParser.Contains.Parse(tokens);

            // Assert
            AssertThatCanParse(expression, expectedContains);
        }

        public static IEnumerable<object[]> OrExpressionCases
        {
            get
            {
                yield return new object[]
                {
                    "Bruce|bat*",
                    new OrExpression(new ConstantValueExpression("Bruce"), new StartsWithExpression("bat"))
                };

                yield return new object[]
                {
                    "Bruce|Wayne",
                    new OrExpression(new ConstantValueExpression("Bruce"), new ConstantValueExpression("Wayne"))
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
            _outputHelper.WriteLine($"input : '{input}'");
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
            _outputHelper.WriteLine($"input : '{input}'");
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
                    new NotExpression(new ConstantValueExpression("Bruce"))
                };

                yield return new object[]
                {
                    "!!Bruce",
                    new NotExpression(new NotExpression(new ConstantValueExpression("Bruce")))
                };

                yield return new object[]
                {
                    "!Bru*",
                    new NotExpression(new StartsWithExpression("Bru"))
                };

                yield return new object[]
                {
                    "![10 TO *[",
                    new NotExpression(new RangeExpression(min : new BoundaryExpression(new ConstantValueExpression("10"), included: true)))
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
            _outputHelper.WriteLine($"input : '{input}'");
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
                    new OneOfExpression(new ConstantValueExpression("B"),new ConstantValueExpression("b"))
                };

                yield return new object[]
                {
                    "[Bb]ruce",
                    new OneOfExpression(new ConstantValueExpression("Bruce"), new ConstantValueExpression("bruce"))
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
                    new OneOfExpression(new ConstantValueExpression("catWoman"), new ConstantValueExpression("catwoman"))
                };

                yield return new object[]
                {
                    "*Br[Uu]",
                    new OneOfExpression(new EndsWithExpression("BrU"), new EndsWithExpression("Bru"))
                };

                yield return new object[]
                {
                    "Bo[Bb]",
                    new OneOfExpression(new ConstantValueExpression("BoB"), new ConstantValueExpression("Bob"))
                };
            }
        }

        [Theory]
        [MemberData(nameof(OneOfExpressionCases))]
        public void CanParseOneOfExpression(string input, OneOfExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
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
                    new RangeExpression(min: new BoundaryExpression(new ConstantValueExpression("10"), included: true),
                                        max: new BoundaryExpression(new ConstantValueExpression("20"), included : true))
                };

                yield return new object[]
                {
                    "]* TO 20]",
                    new RangeExpression(max: new BoundaryExpression(new ConstantValueExpression("20"), included: true))
                };

                yield return new object[]
                {
                    "[10 TO *[",
                    new RangeExpression(min: new BoundaryExpression(new ConstantValueExpression("10"), included : true))
                };

                yield return new object[]
                {
                    "[2010-06-25 TO 2010-06-29]",
                    new RangeExpression(
                        min: new BoundaryExpression(new DateExpression(year: 2010, month: 06, day:25), included: true),
                        max: new BoundaryExpression(new DateExpression(year: 2010, month: 06, day:29), included: true)
                    )
                };

                yield return new object[]
                {
                    "[2010-06-25 TO *[",
                    new RangeExpression(min: new BoundaryExpression(new DateExpression(year: 2010, month: 06, day:25), included : true)
                    )
                };

                yield return new object[]
                {
                    "[13:30:00 TO *[",
                    new RangeExpression(min: new BoundaryExpression(new TimeExpression(hours: 13, minutes: 30), included : true))
                };

                yield return new object[]
                {
                    "]2010-06-25 TO 2010-06-29[",
                    new RangeExpression(
                        min: new BoundaryExpression(new DateExpression(year: 2010, month: 06, day:25), included: false),
                        max: new BoundaryExpression(new DateExpression(year: 2010, month: 06, day:29), included: false)
                    )
                };
            }
        }

        [Theory]
        [MemberData(nameof(RangeCases))]
        public void CanParseRange(string input, RangeExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
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
            _outputHelper.WriteLine($"input : '{input}'");
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

                yield return new object[]
                {
                    @"Henchmen[""Name""]=*rob*",
                    new PropertyNameExpression(@"Henchmen[""Name""]")
                };

                yield return new object[]
                {
                    @"Henchmen[""Powers""][""Description""]=*strength*",
                    new PropertyNameExpression(@"Henchmen[""Powers""][""Description""]")
                };
            }
        }

        [Theory]
        [MemberData(nameof(PropertyNameCases))]
        public void CanParsePropertyNameExpression(string input, PropertyNameExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
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
                        (FilterExpression) new ConstantValueExpression("Vandal")
                    )
                };

                yield return new object[]
                {
                    "Name=Vandal|Banner",
                    (
                        new PropertyNameExpression("Name"),
                        (FilterExpression) new OrExpression(new ConstantValueExpression("Vandal"), new ConstantValueExpression("Banner"))
                    )
                };

                yield return new object[]
                {
                    "Name=Vandal|Banner",
                    (
                        new PropertyNameExpression("Name"),
                        (FilterExpression) new OrExpression(new ConstantValueExpression("Vandal"), new ConstantValueExpression("Banner"))
                    )
                };

                yield return new object[]
                {
                    "Size=[10 TO 20]",
                    (
                        new PropertyNameExpression("Size"),
                        (FilterExpression) new RangeExpression(min: new BoundaryExpression(new ConstantValueExpression("10"),
                                                                                           included: true),
                                                               max: new BoundaryExpression(new ConstantValueExpression("20"),
                                                                                           included: true))
                    )
                };

                yield return new object[]
                {
                    @"Acolytes[""Name""][""Superior""]=Vandal|Banner",
                    (
                        new PropertyNameExpression(@"Acolytes[""Name""][""Superior""]"),
                        (FilterExpression) new OrExpression(new ConstantValueExpression("Vandal"), new ConstantValueExpression("Banner"))
                    )
                };

                yield return new object[]
                {
                    @"Appointment[""Date""]=]2012-10-19T15:03:45Z TO 2012-10-19T15:30:45+01:00[",
                    (
                        new PropertyNameExpression(@"Appointment[""Date""]"),
                        (FilterExpression) new RangeExpression(min: new BoundaryExpression(new DateTimeExpression(new DateExpression(year: 2012, month: 10, day: 19 ),
                                                                                                                  new TimeExpression(hours : 15, minutes: 03, seconds: 45, offset: new TimeOffset())),
                                                                                           included: false),
                                                               max: new BoundaryExpression(new DateTimeExpression(new DateExpression(year: 2012, month: 10, day: 19 ),
                                                                                                                  new TimeExpression(hours : 15, minutes: 30, seconds: 45, offset: new TimeOffset(hours: 1))),
                                                                                           included: false)
                        )
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
            _outputHelper.WriteLine($"{nameof(input)} : '{input}'");

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
                        && expressions.Once(expr => expr.prop.Equals(new PropertyNameExpression("Firstname")) && expr.expression.Equals(new ConstantValueExpression("Vandal")))
                        && expressions.Once(expr => expr.prop.Equals(new PropertyNameExpression("Lastname")) && expr.expression.Equals(new ConstantValueExpression("Savage")))
                    )
                };

                yield return new object[]
                {
                    "Firstname=[Vv]andal",
                    (Expression<Func<IEnumerable<(PropertyNameExpression prop, FilterExpression expression)>, bool>>)(
                        expressions => expressions.Exactly(1)
                        && expressions.Once(expr => expr.prop.Equals(new PropertyNameExpression("Firstname"))
                            && expr.expression.Equals(new OneOfExpression(new ConstantValueExpression("Vandal"), new ConstantValueExpression("vandal"))))
                    )
                };

                foreach (char c in FilterTokenizer.SpecialCharacters)
                {
                    yield return new object[]
                    {
                        $@"Firstname=Vand\{c}al&Lastname=Savage",
                        (Expression<Func<IEnumerable<(PropertyNameExpression prop, FilterExpression expression)>, bool>>)(
                            expressions => expressions.Exactly(2)
                               && expressions.Once(expr => expr.prop.Equals(new PropertyNameExpression("Firstname")) && expr.expression.Equals(new ConstantValueExpression($"Vand{c}al")))
                               && expressions.Once(expr => expr.prop.Equals(new PropertyNameExpression("Lastname")) && expr.expression.Equals(new ConstantValueExpression("Savage")))
                        )
                    };
                }
            }
        }

        /// <summary>
        /// Tests if <see cref="FilterTokenParser.Criteria"/> can parse a criteria
        /// </summary>
        /// <param name="input"></param>
        /// <param name="expectation"></param>
        [Theory]
        [MemberData(nameof(CriteriaCases))]
        public void CanParseCriteria(string input, Expression<Func<IEnumerable<(PropertyNameExpression prop, FilterExpression expression)>, bool>> expectation)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            IEnumerable<(PropertyNameExpression prop, FilterExpression expression)> actual = FilterTokenParser.Criteria.Parse(tokens);

            // Assert
            actual.Should()
                .Match(expectation);
        }

        public static IEnumerable<object[]> DateAndTimeCases
        {
            get
            {
                yield return new object[]
                {
                    "2019-01-12T14:33:00",
                    new DateTimeExpression(
                        new DateExpression(year : 2019, month: 01, day: 12),
                        new TimeExpression(hours: 14, minutes:33)
                    )
                };

                yield return new object[]
                {
                    "2019-01-12 14:33:00",
                    new DateTimeExpression(
                        new DateExpression(year : 2019, month: 01, day: 12),
                        new TimeExpression(hours: 14, minutes:33)
                    )
                };

                yield return new object[]
                {
                    "2019-01-12T14:33:00+01:25",
                    new DateTimeExpression(
                        new DateExpression(year : 2019, month: 01, day: 12),
                        new TimeExpression(hours: 14, minutes:33, offset: new(hours : 1, minutes: 25))
                    )
                };
            }
        }

        [Theory]
        [MemberData(nameof(DateAndTimeCases))]
        public void CanParseDateAndTime(string input, DateTimeExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            DateTimeExpression actual = FilterTokenParser.DateAndTime.Parse(tokens);

            // Assert
            AssertThatCanParse(actual, expected);
        }

        public static IEnumerable<object[]> DateCases
        {
            get
            {
                yield return new object[]
                {
                    "2019-01-12",
                    new DateExpression(year : 2019, month: 01, day: 12)
                };
            }
        }

        [Theory]
        [MemberData(nameof(DateCases))]
        public void CanParseDateCases(string input, DateExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            DateExpression actual = FilterTokenParser.Date.Parse(tokens);

            // Assert
            AssertThatCanParse(actual, expected);
        }

        public static IEnumerable<object[]> TimeCases
        {
            get
            {
                yield return new object[]
                {
                    "18:35:00",
                    new TimeExpression(hours : 18, minutes: 35)
                };

                yield return new object[]
                {
                    "23:59:60",
                    new TimeExpression(hours : 23, minutes: 59, seconds : 60)
                };
            }
        }

        [Theory]
        [MemberData(nameof(TimeCases))]
        public void CanParseTimeExpression(string input, TimeExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            TimeExpression actual = FilterTokenParser.Time.Parse(tokens);

            // Assert
            AssertThatCanParse(actual, expected);
        }

        public static IEnumerable<object[]> DurationCases
        {
            get
            {
                yield return new object[]
                {
                    "P3Y6M4DT12H30M5S",
                    new DurationExpression(years: 3, months: 6, days: 4, hours: 12, minutes: 30, seconds: 5)
                };

                yield return new object[]
                {
                    "PT12H30M5S",
                    new DurationExpression(hours: 12, minutes: 30, seconds: 5)
                };

                yield return new object[]
                {
                    "PT0H",
                    new DurationExpression()
                };
            }
        }

        [Theory]
        [MemberData(nameof(DurationCases))]
        private void CanParseDurationExpression(string input, DurationExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            DurationExpression actual = FilterTokenParser.Duration.Parse(tokens);

            // Assert
            AssertThatCanParse(actual, expected);
        }

        private static void AssertThatCanParse(FilterExpression expression, FilterExpression expected)
            => expression.Should()
                         .NotBeSameAs(expected).And
                         .Be(expected);
    }
}

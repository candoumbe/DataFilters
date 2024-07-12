namespace DataFilters.UnitTests.Grammar.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using DataFilters.Grammar.Parsing;
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;
    using Superpower;
    using Superpower.Model;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    /// <summary>
    /// Unit tests for  <see cref="FilterTokenParser"/>
    /// </summary>
    [UnitTest]
    [Feature(nameof(DataFilters.Grammar.Parsing))]
    [Feature(nameof(FilterTokenParser))]
    public class FilterTokenParserTests : IClassFixture<CultureSwitcher>, IDisposable
    {
        private readonly FilterTokenizer _tokenizer;
        private readonly ITestOutputHelper _outputHelper;
        private readonly CultureSwitcher _cultureSwitcher;

        public FilterTokenParserTests(ITestOutputHelper outputHelper, CultureSwitcher cultureSwitcher)
        {
            _tokenizer = new FilterTokenizer();
            _outputHelper = outputHelper;
            _cultureSwitcher = cultureSwitcher;
            _cultureSwitcher.DefaultCulture = CultureInfo.GetCultureInfo("fr-FR");

            _outputHelper.WriteLine($"Current culture is '{_cultureSwitcher.CurrentCulture}'");
        }

        ///<inheritdoc/>
        public void Dispose() => _cultureSwitcher?.Dispose();

        [Fact]
        public void IsParser() => typeof(FilterTokenParser).Should()
                                                           .BeStatic().And
                                                           .HaveProperty<TokenListParser<FilterToken, ConstantValueExpression>>("AlphaNumeric").And
                                                           .HaveProperty<TokenListParser<FilterToken, PropertyName>>("Property").And
                                                           .HaveProperty<TokenListParser<FilterToken, StartsWithExpression>>("StartsWith").And
                                                           .HaveProperty<TokenListParser<FilterToken, EndsWithExpression>>("EndsWith").And
                                                           .HaveProperty<TokenListParser<FilterToken, OneOfExpression>>("OneOf").And
                                                           .HaveProperty<TokenListParser<FilterToken, ContainsExpression>>("Contains").And
                                                           .HaveProperty<TokenListParser<FilterToken, AsteriskExpression>>("Asterisk").And
                                                           .HaveProperty<TokenListParser<FilterToken, GroupExpression>>("Group").And
                                                           .HaveProperty<TokenListParser<FilterToken, AndExpression>>("And").And
                                                           .HaveProperty<TokenListParser<FilterToken, IntervalExpression>>("Interval").And
                                                           .HaveProperty<TokenListParser<FilterToken, NotExpression>>("Not").And
                                                           .HaveProperty<TokenListParser<FilterToken, NumericValueExpression>>("Number").And
                                                           .HaveProperty<TokenListParser<FilterToken, GuidValueExpression>>("GlobalUniqueIdentifier").And
                                                           .HaveProperty<TokenListParser<FilterToken, FilterExpression>>("Or");

        public static TheoryData<string, ConstantValueExpression> AlphaNumericCases
            => new()
                {
                    {
                        "Bruce",
                        new StringValueExpression("Bruce")
                    },

                    {
                        @"Vandal\*",
                        new StringValueExpression("Vandal*")
                    },

                    {
                        @"Van\*dal",
                        new StringValueExpression("Van*dal")
                    },

                    {
                        "1Bruce",
                        new StringValueExpression("1Bruce")
                    },

                    {
                        @"0\ \,i#y",
                        new StringValueExpression("0 ,i#y")
                    },

                    {
                        @"E
I\&_Oj
\.",
                        new StringValueExpression(@"E
I&_Oj
.")
                    },

                    {
                        "39.95173047301258862",
                        new NumericValueExpression("39.95173047301258862")
                    },
                };

        [Theory]
        [MemberData(nameof(AlphaNumericCases))]
        public void Should_parse_AlphaNumeric(string input, ConstantValueExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);
            _outputHelper.WriteLine($"Tokens : ${StringifyTokens(tokens)}");

            // Act
            FilterExpression actual = FilterTokenParser.AlphaNumeric.Parse(tokens);

            _outputHelper.WriteLine($"actual is '{actual.EscapedParseableString}'");

            // Assert
            AssertThatShould_parse(actual, expected);
        }

        [Property]
        public void Given_double_as_string_Number_should_parse_to_a_ConstantValueExpression_where_Value_property_holds_the_input(NormalFloat value)
        {
            // Arrange
            NumericValueExpression expected = new(value.Item.ToString(CultureInfo.InvariantCulture));
            string input = expected.EscapedParseableString;

            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);
            _outputHelper.WriteLine($"Tokens : ${StringifyTokens(tokens)}");

            // Act
            NumericValueExpression actual = FilterTokenParser.Number.Parse(tokens);

            // Assert
            AssertThatShould_parse(actual, expected);
        }

        [Property]
        public void Given_long_as_string_Number_should_parse_to_a_ConstantValueExpression_where_Value_property_holds_the_input(long value)
        {
            // Arrange
            NumericValueExpression expected = new(value.ToString());
            string input = expected.EscapedParseableString;

            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);
            _outputHelper.WriteLine($"Tokens : ${StringifyTokens(tokens)}");

            // Act
            NumericValueExpression actual = FilterTokenParser.Number.Parse(tokens);

            // Assert
            AssertThatShould_parse(actual, expected);
        }

        [Property]
        public void Given_int_as_string_Number_should_parse_to_a_ConstantValueExpression_where_Value_property_holds_the_input(int value)
        {
            // Arrange
            NumericValueExpression expected = new(value.ToString());
            string input = expected.EscapedParseableString;

            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);
            _outputHelper.WriteLine($"Tokens : ${StringifyTokens(tokens)}");

            // Act
            NumericValueExpression actual = FilterTokenParser.Number.Parse(tokens);

            // Assert
            AssertThatShould_parse(actual, expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Should_parse_StartsWith(StartsWithExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{expected.EscapedParseableString}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString);
            _outputHelper.WriteLine($"Tokens : ${StringifyTokens(tokens)}");

            // Act
            StartsWithExpression expression = FilterTokenParser.StartsWith.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Should_parse_EndsWith(EndsWithExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{expected.EscapedParseableString}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString);
            _outputHelper.WriteLine($"Tokens : ${StringifyTokens(tokens)}");

            // Act
            EndsWithExpression expression = FilterTokenParser.EndsWith.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
        }

        private static string StringifyTokens(TokenList<FilterToken> tokens)
            => tokens.Select(token => new { token.Kind, Value = token.ToStringValue() }).Jsonify();

        public static TheoryData<string, ContainsExpression> ContainsCases
        {
            get
            {
                TheoryData<string, ContainsExpression> cases = new()
                {
                    { "*bat*", new ContainsExpression("bat")},
                    { @"*bat\*man*", new ContainsExpression("bat*man")},
                    {"*d3aa022d-ec52-47aa-be13-6823c478c60a*", new ContainsExpression("d3aa022d-ec52-47aa-be13-6823c478c60a")}
                };

                string[] punctuations = [".", "-", ":", "_"];

                foreach (string punctuation in punctuations)
                {
                    cases.Add
                    (
                        $"*{punctuation}*",
                        new ContainsExpression(punctuation)
                    );
                }

                return cases;
            }
        }

        [Theory]
        [MemberData(nameof(ContainsCases))]
        public void Should_parse_Contains(string input, ContainsExpression expectedContains)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            ContainsExpression expression = FilterTokenParser.Contains.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expectedContains);
        }

        public static TheoryData<string, OrExpression> OrExpressionCases
            => new()
            {
                {
                    "Bruce|bat*",
                    new OrExpression(new StringValueExpression("Bruce"), new StartsWithExpression("bat"))
                },
                {
                    "Bruce|Wayne",
                    new OrExpression(new StringValueExpression("Bruce"), new StringValueExpression("Wayne"))
                },
                {
                    "Bru*|Di*",
                    new OrExpression(new StartsWithExpression("Bru"), new StartsWithExpression("Di"))
                },
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
                },
                {
                    "Bat|Wonder|Sup",
                    new OrExpression(new OrExpression(new StringValueExpression("Bat"),
                                                      new StringValueExpression("Wonder")),
                                    new StringValueExpression("Sup"))
                }
            };

        [Theory]
        [MemberData(nameof(OrExpressionCases))]
        public void Should_parse_OrExpression(string input, OrExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            FilterExpression expression = FilterTokenParser.Or.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
        }

        public static TheoryData<string, AndExpression> AndExpressionCases
            => new()
            {
                {
                    "bat*,man*",
                    new AndExpression(new StartsWithExpression("bat"), new StartsWithExpression("man"))
                },
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
                },
                {
                    "bat*man",
                    new AndExpression(new StartsWithExpression("bat"), new EndsWithExpression("man"))
                }
            };

        [Theory]
        [MemberData(nameof(AndExpressionCases))]
        public void Should_parse_AndExpression(string input, AndExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            FilterExpression expression = FilterTokenParser.And.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Should_parse_NotExpression(NotExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"source : '{expected}'");
            _outputHelper.WriteLine($"input (escaped parseable string): '{expected.EscapedParseableString}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString);
            _outputHelper.WriteLine($"Tokens : '{StringifyTokens(tokens)}'");

            // Act
            NotExpression expression = FilterTokenParser.Not.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
        }

        public static TheoryData<string, NotExpression> NotParsingCases
            => new()
            {
                {
                    "!!!!!(w%A*@,2088-08-10)",
                    new NotExpression(
                        new NotExpression(
                            new NotExpression(
                                new NotExpression(
                                    new NotExpression(
                                            new GroupExpression(
                                                    new AndExpression(
                                                    new AndExpression(new StartsWithExpression("w%A"),
                                                                      new EndsWithExpression("@")),
                                                    new DateExpression(2088, 8, 10)
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                },
                {
                    "!![50 TO 50]",
                    new NotExpression
                        (new NotExpression(
                                new IntervalExpression(min: new (new NumericValueExpression("50"), true),
                                                                               new(new NumericValueExpression("60"), true))))
                }
            };

        [Theory]
        [MemberData(nameof(NotParsingCases))]
        public void Given_NotExpression_as_parseable_string_Not_should_parse_the_input(string input, NotExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString);

            // Act
            NotExpression expression = FilterTokenParser.Not.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
        }

        public static TheoryData<string, OneOfExpression> OneOfExpressionCases
            => new()
            {
                {
                    "[Bb]",
                    new OneOfExpression(new StringValueExpression("B"), new StringValueExpression("b"))
                },

                {
                    "[Bb]ruce",
                    new OneOfExpression(new StringValueExpression("Bruce"), new StringValueExpression("bruce"))
                },

                {
                    "[Bb]at*",
                    new OneOfExpression(new StartsWithExpression("Bat"), new StartsWithExpression("bat"))
                },

                {
                    "Ma*[Nn]",
                    new OneOfExpression(
                        new AndExpression(new StartsWithExpression("Ma"), new EndsWithExpression("N")),
                        new AndExpression(new StartsWithExpression("Ma"), new EndsWithExpression("n"))
                    )
                },

                {
                    "cat[Ww]oman",
                    new OneOfExpression(new StringValueExpression("catWoman"), new StringValueExpression("catwoman"))
                },

                {
                    "*Br[Uu]",
                    new OneOfExpression(new EndsWithExpression("BrU"), new EndsWithExpression("Bru"))
                },

                {
                    "Bo[Bb]",
                    new OneOfExpression(new StringValueExpression("BoB"), new StringValueExpression("Bob"))
                },

                {
                    "[Bb]*ob",
                    new OneOfExpression(new AndExpression(new StartsWithExpression("B"), new EndsWithExpression("ob")),
                                        new AndExpression(new StartsWithExpression("b"), new EndsWithExpression("ob")))
                },

                {
                    "*[Mm]an",
                    new OneOfExpression(new EndsWithExpression("Man"), new EndsWithExpression("man"))
                },

                {
                    "*[Mm]",
                    new OneOfExpression(new EndsWithExpression("M"), new EndsWithExpression("m"))
                },

                {
                    "[a-z]",
                    new OneOfExpression([ .. GetCharacters('a', 'z').Select(chr => new StringValueExpression(chr.ToString())) ])
                },

                {
                    "[a-zA-Z0-9]",
                    new OneOfExpression([.. GetCharacters('a', 'z').Concat(GetCharacters('A', 'Z'))
                                                               .Concat(GetCharacters('0', '9'))
                                                               .Select(chr => new StringValueExpression(chr.ToString()))
                                                               ])
                },

                {
                    "{Bat|Sup|Wonder}*",
                    new OneOfExpression(new StartsWithExpression("Bat"),
                                        new StartsWithExpression("Sup"),
                                        new StartsWithExpression("Wonder"))
                },

                {
                    "[ab][cd]",
                    new OneOfExpression(new StringValueExpression("ac"),
                                        new StringValueExpression("ad"),
                                        new StringValueExpression("bc"),
                                        new StringValueExpression("bd"))
                },

                {
                    "{Bat|Sup|Wonder}",
                    new OneOfExpression(new StringValueExpression("Bat"),
                                        new StringValueExpression("Sup"),
                                        new StringValueExpression("Wonder"))
                }
            };

        private static IEnumerable<char> GetCharacters(char regexStart, char regexEnd)
            => Enumerable.Range(regexStart, regexEnd - regexStart + 1)
                         .Select(ascii => (char)ascii);

        [Theory]
        [MemberData(nameof(OneOfExpressionCases))]
        public void Should_parse_OneOfExpression(string input, OneOfExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            OneOfExpression expression = FilterTokenParser.OneOf.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_bracket_expression_OneOf_can_parse_input(NonNull<BracketValue> bracketExpression)
        {
            // Arrange
            string input = bracketExpression.Item.EscapedParseableString;
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            OneOfExpression expected = bracketExpression.Item switch
            {
                ConstantBracketValue constant => new([.. constant.Value.Select(chr => new StringValueExpression(chr.ToString()))]),
                RangeBracketValue range => new([.. Enumerable.Range(range.Start, range.End - range.Start + 1)
                                                         .Select(ascii => (char)ascii)
                                                         .Select(chr => new StringValueExpression(chr.ToString()))]),
                _ => throw new NotSupportedException()
            };

            // Act
            OneOfExpression expression = FilterTokenParser.OneOf.Parse(tokens);

            // Assert
            expression.IsEquivalentTo(expected).ToProperty();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Should_parse_Interval(CultureInfo culture, IntervalExpression expected)
        {
            _cultureSwitcher.Run(culture, () =>
            {
                // Arrange
                _outputHelper.WriteLine($"Culture : '{culture.Name}'");
                _outputHelper.WriteLine($"input : '{expected.EscapedParseableString}'");
                TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString);

                // Act
                IntervalExpression expression = FilterTokenParser.Interval.Parse(tokens);

                // Assert
                AssertThatShould_parse(expression, expected);
            });
        }

        public static TheoryData<string, string, IntervalExpression> IntervalCases
        {
            get
            {
                TheoryData<string, string, IntervalExpression> cases = [];
                string[] cultures = ["fr-FR", "en-GB", "en-US"];
                foreach (string culture in cultures)
                {
                    cases.Add
                    (
                        culture,
                        "[5 TO 5[",
                        new IntervalExpression(new BoundaryExpression(new NumericValueExpression("5"), true), new BoundaryExpression(new NumericValueExpression("5"), false))
                    );

                    cases.Add
                    (
                        culture,
                        "[1993-08-04T00:25:05.155Z TO 1908-06-08T03:18:46.745-09:50[",
                        new IntervalExpression(new BoundaryExpression(new DateTimeExpression(new(1993, 8, 4), new(0, 25, 5, 155), OffsetExpression.Zero), true),
                                               new BoundaryExpression(new DateTimeExpression(new(1908, 6, 8), new(3, 18, 46, 745), new(NumericSign.Minus, 9, 50)), false))
                    );

                    cases.Add
                    (
                        culture,
                        "]* TO 1963-06-03T15:53:44.609Z]",
                        new IntervalExpression(max: new BoundaryExpression(new DateTimeExpression(new(1963, 6, 3), new(15, 53, 44, 609), OffsetExpression.Zero), true))
                    );

                    cases.Add
                    (
                        culture,
                        "]2E-05 TO 32[",
                        new IntervalExpression(
                            min: new BoundaryExpression(new NumericValueExpression("2E-05"), included: false),
                            max: new BoundaryExpression(new NumericValueExpression("32"), included: false))
                    );
                    cases.Add
                    (
                        culture,
                        "]-2E-05 TO 32[",
                        new IntervalExpression(
                            min: new BoundaryExpression(new NumericValueExpression("-2E-05"), included: false),
                            max: new BoundaryExpression(new NumericValueExpression("32"), included: false))
                    );
                    cases.Add
                    (
                        culture,
                        "]+2E-05 TO 32[",
                        new IntervalExpression(
                            min: new BoundaryExpression(new NumericValueExpression("2E-05"), included: false),
                            max: new BoundaryExpression(new NumericValueExpression("32"), included: false))
                    );
                }

                return cases;
            }
        }

        [Theory]
        [MemberData(nameof(IntervalCases))]
        public void Given_interval_as_string_Parser_should_parse_to_IntervalExpression(string culture, string input, IntervalExpression expected)
        {
            _cultureSwitcher.Run(culture, () =>
            {
                // Arrange
                _outputHelper.WriteLine($"input : '{input}'");
                TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

                _outputHelper.WriteLine($"Tokens : {StringifyTokens(tokens)}");

                // Act
                IntervalExpression expression = FilterTokenParser.Interval.Parse(tokens);

                // Assert
                AssertThatShould_parse(expression, expected);
            });
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Should_parse_text(CultureInfo culture, TextExpression expected)
        {
            _cultureSwitcher.Run(culture, () =>
            {
                // Arrange
                _outputHelper.WriteLine($"Culture : '{culture.Name}'");
                _outputHelper.WriteLine($"input : '{expected.EscapedParseableString}'");
                TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString);

                // Act
                TextExpression actual = FilterTokenParser.Text.Parse(tokens);

                // Assert
                AssertThatShould_parse(actual, expected);
            });
        }

        public static TheoryData<string, PropertyName> PropertyNameCases
            => new()
            {
                {
                    "Firstname=Bruce",
                    new PropertyName("Firstname")
                },

                {
                    @"Henchmen[""Name""]=*rob*",
                    new PropertyName(@"Henchmen[""Name""]")
                },

                {
                    @"Henchmen[""Powers""][""Description""]=*strength*",
                    new PropertyName(@"Henchmen[""Powers""][""Description""]")
                },

                {
                    @"Henchmen[""first_name""]=*rob*",
                    new PropertyName(@"Henchmen[""first_name""]")
                },

                {
                    "first_name=Bruce",
                    new PropertyName("first_name")
                },
            };

        [Theory]
        [MemberData(nameof(PropertyNameCases))]
        public void Should_parse_PropertyNameExpression(string input, PropertyName expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            PropertyName expression = FilterTokenParser.Property.Parse(tokens);

            // Assert
            expression.Should()
                      .NotBeSameAs(expected).And
                      .Be(expected);
        }

        public static TheoryData<string, (PropertyName prop, FilterExpression expression)> CriterionCases
            => new()
            {
                {
                    "Name=Vandal",
                    (
                        new PropertyName("Name"),
                         new StringValueExpression("Vandal")
                    )
                },
                {
                    "first_name=Vandal",
                    (
                        new PropertyName("first_name"),
                         new StringValueExpression("Vandal")
                    )
                },
                {
                    "Name=Vandal|Banner",
                    (
                        new PropertyName("Name"),
                         new OrExpression(new StringValueExpression("Vandal"), new StringValueExpression("Banner"))
                    )
                },
                {
                    "Name=Vandal|Banner",
                    (
                        new PropertyName("Name"),
                         new OrExpression(new StringValueExpression("Vandal"), new StringValueExpression("Banner"))
                    )
                },
                {
                    "Size=[10 TO 20]",
                    (
                        new PropertyName("Size"),
                         new IntervalExpression(min: new BoundaryExpression(new NumericValueExpression("10"),
                                                                                           included: true),
                                                               max: new BoundaryExpression(new NumericValueExpression("20"),
                                                                                           included: true))
                    )
                },
                {
                    @"Acolytes[""Name""][""Superior""]=Vandal|Banner",
                    (
                        new PropertyName(@"Acolytes[""Name""][""Superior""]"),
                         new OrExpression(new StringValueExpression("Vandal"), new StringValueExpression("Banner"))
                    )
                },
                {
                    @"Appointment[""Date""]=]2012-10-19T15:03:45Z TO 2012-10-19T15:30:45+01:00[",
                    (
                        new PropertyName(@"Appointment[""Date""]"),
                         new IntervalExpression(min: new BoundaryExpression(new DateTimeExpression(new DateExpression(year: 2012, month: 10, day: 19 ),
                                                                                                                               new TimeExpression(hours : 15, minutes: 03, seconds: 45),
                                                                                                                               OffsetExpression.Zero),
                                                                                              included: false),
                                                                  max: new BoundaryExpression(new DateTimeExpression(new DateExpression(year: 2012, month: 10, day: 19 ),
                                                                                                                              new TimeExpression(hours : 15, minutes: 30, seconds: 45),
                                                                                                                              new OffsetExpression(hours: 1)),
                                                                                              included: false)
                        )
                    )
                },
                {
                    "Name=*[Mm]an",
                    (
                        new PropertyName("Name"),
                         new OneOfExpression(new EndsWithExpression("Man"),
                                                               new EndsWithExpression("man"))
                    )
                },
                {
                    "Name=[Bb]o[Bb]",
                    (
                        new PropertyName("Name"),
                        new OneOfExpression(new StringValueExpression("BoB"),
                                                              new StringValueExpression("Bob"),
                                                              new StringValueExpression("boB"),
                                                              new StringValueExpression("bob"))
                    )
                },
                {
                    "prop=!(((1908-09-30T00:31:33.127+05:13|2000-06-19)|(00:01:40.443|*))|((2003-05-27|89ae5244-2a98-16cc-6f99-d17658594604)|(s*|*0$)))",
                    (
                        new PropertyName("prop"),
                        new NotExpression(
                                new OrExpression(
                                new OrExpression(

                                        new OrExpression(new DateTimeExpression(new DateExpression(1908, 9, 30),
                                                                                          new TimeExpression(0, 31, 33, 127),
                                                                                          new OffsetExpression(hours: 5, minutes: 13)),
                                                              new DateExpression(2000, 6, 19)),
                                        new OrExpression(new TimeExpression(minutes: 1, seconds: 40, milliseconds: 443),
                                                              new EndsWithExpression(""))
                                    ),
                                        new OrExpression(new OrExpression(new DateExpression(2003, 5, 27),
                                                                                    new GuidValueExpression("89ae5244-2a98-16cc-6f99-d17658594604")),
                                                              new OrExpression(new StartsWithExpression("s"), new EndsWithExpression("0$"))
                                        )
                                ))
                    )
                },
                {
                    "Nickname={Bat|Sup|Wonder}*",
                    (
                        new PropertyName("Nickname"),
                        new OneOfExpression(new StartsWithExpression("Bat"),
                                            new StartsWithExpression("Sup"),
                                            new StartsWithExpression("Wonder"))
                    )
                },

                {
                    "Value=!!5",
                    (
                        new PropertyName("Value"),
                        new NotExpression(new NotExpression(new NumericValueExpression("5")))
                    )
                }
            };

        /// <summary>
        /// Tests if <see cref="FilterTokenParser.Criteria"/> can parse a criteria
        /// </summary>
        /// <param name="input"></param>
        /// <param name="expected"></param>
        [Theory]
        [MemberData(nameof(CriterionCases))]
        public void Should_parse_Criterion(string input, (PropertyName prop, FilterExpression expression) expected)
        {
            _outputHelper.WriteLine($"{nameof(input)} : '{input}'");

            // Arrange
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            (PropertyName prop, FilterExpression expression) = FilterTokenParser.Criterion.Parse(tokens);

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

        public static TheoryData<string, Expression<Func<IEnumerable<(PropertyName prop, FilterExpression expression)>, bool>>> CriteriaCases
        {
            get
            {
                TheoryData<string, Expression<Func<IEnumerable<(PropertyName prop, FilterExpression expression)>, bool>>> cases = new()
                {
                    {
                        "Firstname=Vandal&Lastname=Savage",
                        expressions => expressions.Exactly(2)
                            && expressions.Once(expr => expr.prop.Equals(new PropertyName("Firstname")) && expr.expression.Equals(new StringValueExpression("Vandal")))
                            && expressions.Once(expr => expr.prop.Equals(new PropertyName("Lastname")) && expr.expression.Equals(new StringValueExpression("Savage")))

                    },
                    {
                        "Firstname=[Vv]andal",
                        expressions => expressions.Exactly(1)
                            && expressions.Once(expr => expr.prop.Equals(new PropertyName("Firstname"))
                                && expr.expression.Equals(new OneOfExpression(new StringValueExpression("Vandal"),
                                                                                  new StringValueExpression("vandal"))))
                    }
                };

                foreach (char c in FilterTokenizer.SpecialCharacters)
                {
                    cases.Add
                    (
                        $@"Firstname=Vand\{c}al&Lastname=Savage",
                        expressions => expressions.Exactly(2)
                               && expressions.Once(expr => expr.prop.Equals(new PropertyName("Firstname")) && expr.expression.Equals(new StringValueExpression($"Vand{c}al")))
                               && expressions.Once(expr => expr.prop.Equals(new PropertyName("Lastname")) && expr.expression.Equals(new StringValueExpression("Savage")))
                    );
                }

                foreach (char c in FilterTokenizer.SpecialCharacters)
                {
                    cases.Add
                    (
                        $@"first_name=Vand\{c}al&last_name=Savage",
                        expressions => expressions.Exactly(2)
                               && expressions.Once(expr => expr.prop.Equals(new PropertyName("first_name")) && expr.expression.Equals(new StringValueExpression($"Vand{c}al")))
                               && expressions.Once(expr => expr.prop.Equals(new PropertyName("last_name")) && expr.expression.Equals(new StringValueExpression("Savage")))
                    );
                }

                return cases;
            }
        }

        /// <summary>
        /// Tests if <see cref="FilterTokenParser.Criteria"/> can parse a criteria
        /// </summary>
        /// <param name="input"></param>
        /// <param name="expectation"></param>
        [Theory]
        [MemberData(nameof(CriteriaCases))]
        public void Should_parse_Criteria(string input, Expression<Func<IEnumerable<(PropertyName prop, FilterExpression expression)>, bool>> expectation)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            IEnumerable<(PropertyName prop, FilterExpression expression)> actual = FilterTokenParser.Criteria.Parse(tokens);

            // Assert
            actual.Should()
                .Match(expectation);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Should_parse_DateAndTime(DateTimeExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{expected.EscapedParseableString}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString);

            // Act
            DateTimeExpression actual = FilterTokenParser.DateAndTime.Parse(tokens);

            // Assert
            AssertThatShould_parse(actual, expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Should_parse_DateCases(DateExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{expected.EscapedParseableString}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString);

            // Act
            DateExpression actual = FilterTokenParser.Date.Parse(tokens);

            // Assert
            AssertThatShould_parse(actual, expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Should_parse_Groups(GroupExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{expected.EscapedParseableString}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString);

            // Act
            GroupExpression actual = FilterTokenParser.Group.Parse(tokens);

            // Assert
            AssertThatShould_parse(actual, expected);
        }

        public static TheoryData<GroupExpression, string> ParseGroupCases
        {
            get
            {
                TheoryData<GroupExpression, string> cases = [];
                string[] cultures = ["fr-FR", "en-GB", "en-US"];
                foreach (string culture in cultures)
                {
                    cases.Add
                    (
                        new GroupExpression(new DateTimeExpression(new(2090, 10, 10), new(3, 0, 40, 583), OffsetExpression.Zero)),
                        culture
                    );

                    cases.Add
                    (
                        new GroupExpression(new DateTimeExpression(new(2010, 06, 02), new(23, 45, 54, 331), OffsetExpression.Zero)),
                        culture
                    );
                }

                return cases;
            }
        }

        [Theory]
        [MemberData(nameof(ParseGroupCases))]
        public void Given_GroupExpression_EscapedParseableString_as_input_Parser_should_return_GroupExpression_that_is_equivalent_to_input(GroupExpression expected, string culture)
        {
            _cultureSwitcher.Run(culture, () =>
            {
                _outputHelper.WriteLine($"Current culture is '{_cultureSwitcher.CurrentCulture}'");
                _outputHelper.WriteLine($"input : '{expected.EscapedParseableString}'");
                TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString);

                // Act
                GroupExpression actual = FilterTokenParser.Group.Parse(tokens);

                // Assert
                AssertThatShould_parse(actual, expected);
            });
        }

        public static TheoryData<CultureInfo, NumericValueExpression> ParseNumberCases
            => new ()
            {
                {
                    CultureInfo.InvariantCulture,
                    new NumericValueExpression(float.MinValue.ToString(CultureInfo.InvariantCulture))
                },
                {
                    CultureInfo.InvariantCulture,
                    new NumericValueExpression(float.MaxValue.ToString(CultureInfo.InvariantCulture))
                },
                {
                    CultureInfo.InvariantCulture,
                    new NumericValueExpression("2E-05")
                }
            };
        
        [Theory]
        [MemberData(nameof(ParseNumberCases))]
        public void Should_parse_Number(CultureInfo culture, NumericValueExpression expected)
        {
            _cultureSwitcher.Run(culture, () =>
            {
                _outputHelper.WriteLine($"Current culture is '{_cultureSwitcher.CurrentCulture}'");
                _outputHelper.WriteLine($"input : '{expected.EscapedParseableString}'");
                TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString);

                // Act
                NumericValueExpression actual = FilterTokenParser.Number.Parse(tokens);

                // Assert
                AssertThatShould_parse(actual, expected);
            });
        }


        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Should_parse_TimeExpression(NonNull<TimeExpression> expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{expected.Item.EscapedParseableString}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.Item.EscapedParseableString);

            // Act
            TimeExpression actual = FilterTokenParser.Time.Parse(tokens);

            // Assert
            AssertThatShould_parse(actual, expected.Item);
        }
        
        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Should_parse_DurationExpression(DurationExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{expected.EscapedParseableString}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString);

            // Act
            DurationExpression actual = FilterTokenParser.Duration.Parse(tokens);

            // Assert
            AssertThatShould_parse(actual, expected);
        }

        [Theory]
        [InlineData("P", "time separator and duration unit are missing")]
        [InlineData("P1Y", "the time separator 'T' is missing after the 'year' duration")]
        [InlineData("P0S", "the time separator 'T' is missing before the 'second' duration")]
        [InlineData("PT", "the duration string does not contain any duration")]
        public void Given_incorrect_input_for_duration_Parser_should_throw_ParseException(string input, string reason)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input);

            // Act
            Action callingParserWithInvalidStringInput = () => FilterTokenParser.Duration.Parse(tokens);

            // Assert
            AssertThatParseExceptionIsThrown(callingParserWithInvalidStringInput, reason);
        }

        private static void AssertThatShould_parse(FilterExpression actual, FilterExpression expected, string reason = "")
            => actual.Should()
                     .NotBeSameAs(expected).And
                     .Be(expected, reason);

        private static void AssertThatParseExceptionIsThrown(Action action, string reason)
        {
            // Act
            action.Should().ThrowExactly<ParseException>(reason);
        }
    }
}

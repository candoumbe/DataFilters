using DataFilters.ValueObjects;

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
        private static readonly Bogus.Faker Faker = new();

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
                                                           .HaveProperty<TokenListParser<FilterToken, OrExpression>>("Or");

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

                    {
                        "5aa8f391",
                        new StringValueExpression("5aa8f391")
                    },
                    {
                        "6P%",
                        new StringValueExpression("6P%")
                    },
                    {
                        @"gHuQ>a0\!>\:	\-\021+o\]AL2oVmf\029\!",
                        new StringValueExpression("gHuQ>a0!>:\t-\\021+o]AL2oVmf\\029!")
                    }
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
            string input = expected.EscapedParseableString.Value;
            _outputHelper.WriteLine($"Input : {input}");
            
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
            string input = expected.EscapedParseableString.Value;

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
            string input = expected.EscapedParseableString.Value;

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
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString.Value);
            _outputHelper.WriteLine($"Tokens : ${StringifyTokens(tokens)}");

            // Act
            StartsWithExpression expression = FilterTokenParser.StartsWith.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)], Replay = "(1753852137964623312,18335031791097947631)")]
        public void Should_parse_EndsWith(EndsWithExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{expected.EscapedParseableString}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString.Value);
            _outputHelper.WriteLine($"Tokens : ${StringifyTokens(tokens)}");

            // Act
            EndsWithExpression expression = FilterTokenParser.EndsWith.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
        }

        public static TheoryData<EscapedString, EndsWithExpression> EndsWithData
            => new()
            {
                {
                    EscapedString.From(@"*gHuQ>a0\!>\:	\-\021+o\]AL2oVmf\029\!"),
                    new EndsWithExpression(EscapedString.From(@"*gHuQ>a0\!>\:	\-\021+o\]AL2oVmf\029\!"))
                }
            };

        [Theory]
        [MemberData(nameof(EndsWithData))]
        public void Given_ends_with_input_EndsWith_should_return_expected_EndsWithExpression(EscapedString input, EndsWithExpression expected)
        {
            // Arrange
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input.Value);
            _outputHelper.WriteLine($"Tokens : ${StringifyTokens(tokens)}");

            // Act
            EndsWithExpression expression = FilterTokenParser.EndsWith.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
        }
        
        private static string StringifyTokens(TokenList<FilterToken> tokens)
            => tokens.Select(token => new { Kind = token.Kind.ToString(), Value = token.ToStringValue() }).Jsonify();

        public static TheoryData<string, ContainsExpression> ContainsCases
        {
            get
            {
                TheoryData<string, ContainsExpression> cases = new()
                {
                    { "*bat*", new ContainsExpression("bat")},
                    { @"*bat\*man*", new ContainsExpression("bat*man")},
                    {"*d3aa022d-ec52-47aa-be13-6823c478c60a*", new ContainsExpression("d3aa022d-ec52-47aa-be13-6823c478c60a")},
                    {"*\"Oi\012j8G]t:JK%H6m>+r{)[5\n6\"*", new ContainsExpression(EscapedString.From("\"Oi\012j8G]t:JK%H6m>+r{)[5\n6\""))}
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
            _outputHelper.WriteLine($"Tokens : ${StringifyTokens(tokens)}");

            // Act
            ContainsExpression expression = FilterTokenParser.Contains.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expectedContains);
        }

        public static TheoryData<EscapedString, OrExpression> OrExpressionCases
            => new()
            {
                {
                    EscapedString.From("Bruce|bat*"),
                    new OrExpression(new StringValueExpression("Bruce"), new StartsWithExpression("bat"))
                },
                {
                    EscapedString.From("Bruce|Wayne"),
                    new OrExpression(new StringValueExpression("Bruce"), new StringValueExpression("Wayne"))
                },
                {
                    EscapedString.From("Bru*|Di*"),
                    new OrExpression(new StartsWithExpression("Bru"), new StartsWithExpression("Di"))
                },
                {
                    EscapedString.From("(!(Bat*|Sup*))|!*man"),
                    new OrExpression(
                        left : new GroupExpression(
                                new NotExpression(
                                    new GroupExpression(
                                        new OrExpression(new StartsWithExpression("Bat"), new StartsWithExpression("Sup"))
                                    )
                                )
                            ),
                        right: new NotExpression(new EndsWithExpression(EscapedString.From("man")))
                    )
                },
                {
                    EscapedString.From("(Bat|Wonder)|Sup"),
                    new OrExpression(new OrExpression(new StringValueExpression("Bat"),
                                                      new StringValueExpression("Wonder")),
                                    new StringValueExpression("Sup"))
                },

                {
                    (new StringValueExpression("5aa8f391") | new GroupExpression(new GroupExpression(new GroupExpression(new StringValueExpression("6P%"))))).EscapedParseableString,
                    new OrExpression(
                        new StringValueExpression("5aa8f391"),
                        new GroupExpression(new GroupExpression(new GroupExpression(new StringValueExpression("6P%"))))
                    )
                },

                {
                    (new StringValueExpression("5aa8f391") | new StringValueExpression("6P%")).EscapedParseableString,
                    new StringValueExpression("5aa8f391") | new StringValueExpression("6P%")
                },
                
                {
                    (new StringValueExpression("AB") | new StringValueExpression("6P%")).EscapedParseableString,
                    new StringValueExpression("AB") | new StringValueExpression("6P%")
                },
                {
                    (new EndsWithExpression(EscapedString.From("Foo")) | new ContainsExpression("Bar")).EscapedParseableString,
                    new EndsWithExpression(EscapedString.From("Foo")) | new ContainsExpression("Bar")
                },
                {
                    (new EndsWithExpression(EscapedString.From("Foo")) | new EndsWithExpression(EscapedString.From("Bar"))).EscapedParseableString,
                    new EndsWithExpression(EscapedString.From("Foo")) | new EndsWithExpression(EscapedString.From("Bar"))
                },
                {
                    (new StartsWithExpression("Foo") | new EndsWithExpression(EscapedString.From("Bar"))).EscapedParseableString,
                    new StartsWithExpression("Foo") | new EndsWithExpression(EscapedString.From("Bar"))
                },
                {
                    EscapedString.From(@"*N\=FW|i*"),
                    new EndsWithExpression(RawString.From("N=FW")) | new StartsWithExpression("i")
                },
            };

        [Theory]
        [MemberData(nameof(OrExpressionCases))]
        public void Should_parse_OrExpression(EscapedString input, OrExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input.Value);
            
            _outputHelper.WriteLine(StringifyTokens(tokens));

            // Act
            FilterExpression expression = FilterTokenParser.Or.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
        }

        public static TheoryData<EscapedString, AndExpression> AndExpressionCases
            => new()
            {
                {
                    EscapedString.From("bat*,man*"),
                    new AndExpression(new StartsWithExpression("bat"), new StartsWithExpression("man"))
                },
                {
                    EscapedString.From("*bat,man*"),
                    new EndsWithExpression(EscapedString.From("bat")) & new StartsWithExpression("man")
                },
                {
                    EscapedString.From("(!(Bat*|Sup*)),!*man"),
                    new AndExpression(
                        left: new GroupExpression
                        (
                            new NotExpression(
                                new GroupExpression(
                                    new OrExpression(new StartsWithExpression("Bat"), new StartsWithExpression("Sup"))
                                )
                            )
                        ),
                        right: ! new EndsWithExpression(EscapedString.From("man"))
                    )
                },
                {
                    EscapedString.From("bat*man"),
                    new AndExpression(new StartsWithExpression("bat"), new EndsWithExpression(EscapedString.From("man")))
                },
                {
                    EscapedString.From(@"bat*\)"),
                    new AndExpression(new StartsWithExpression("bat"), new EndsWithExpression(RawString.From(")")))
                },
                {
                    EscapedString.From("bat*man*"),
                    new AndExpression(new StartsWithExpression("bat"), new ContainsExpression("man"))
                },
                {
                    EscapedString.From(@"*""1.\""Foo!"",*""2.\""Bar!""*"),
                    new AndExpression(
                        AsteriskExpression.Instance + new TextExpression(@"1.""Foo!"),
                        AsteriskExpression.Instance + new TextExpression(@"2.""Bar!") + AsteriskExpression.Instance
                    )
                }
            };

        [Theory]
        [MemberData(nameof(AndExpressionCases))]
        public void Should_parse_AndExpression(EscapedString input, AndExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input.Value);
            _outputHelper.WriteLine($"Tokens : {StringifyTokens(tokens)}");

            // Act
            FilterExpression expression = FilterTokenParser.And.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Should_parse_NotExpression(NotExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{expected.EscapedParseableString}' ");
            _outputHelper.WriteLine($"input (debug view) : '{expected:d}' ");
            _outputHelper.WriteLine($"input (full view) : '{expected:f}' ");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString.Value);
            
            // Act
            NotExpression expression = FilterTokenParser.Not.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
            
        }

        public static TheoryData<EscapedString, NotExpression> NotParsingCases
            => new()
            {   
                {
                    EscapedString.From("!!(w%A*@,2088-08-10)"),
                    new NotExpression(
                        new NotExpression(
                            new GroupExpression(
                                new AndExpression(
                                    new StartsWithExpression("w%A") + new EndsWithExpression(EscapedString.From("@")),
                                    new DateExpression(2088, 8, 10)
                                )
                            )
                        )
                    )
                },
                {
                    EscapedString.From("!!!!!(w%A*@,2088-08-10)"),
                    !!!!!new GroupExpression(
                                new AndExpression(
                                new StartsWithExpression("w%A") + new EndsWithExpression(EscapedString.From("@")),
                                new DateExpression(2088, 8, 10)
                                )
                        )
                },
                {
                    EscapedString.From("!![50 TO 60]"),
                    !!new IntervalExpression(min: new BoundaryExpression(new NumericValueExpression("50"), true),
                                             max: new BoundaryExpression(new NumericValueExpression("60"), true))
                },
                
                {
                    EscapedString.From("!((word))"),
                    !new GroupExpression
                    (
                        new GroupExpression
                        (
                            new StringValueExpression("word")
                        )
                    )
                },

                {
                    EscapedString.From("!((1)|(2))"),
                    !new GroupExpression(
                        new OrExpression(
                        new GroupExpression(
                                new NumericValueExpression("1")
                            ),
                            new GroupExpression(
                                    new NumericValueExpression("2")
                                )
                        )
                    )
                },
                {
                    (! (new StringValueExpression("5aa8f391") | new GroupExpression(new GroupExpression(new GroupExpression(new StringValueExpression("6P%")))))).EscapedParseableString,
                    ! (new StringValueExpression("5aa8f391") | new GroupExpression(new GroupExpression(new GroupExpression(new StringValueExpression("6P%")))))
                },
                {
                    (! (new StringValueExpression("5aa8f391") | new StringValueExpression("6P%"))).EscapedParseableString,
                    ! (new StringValueExpression("5aa8f391") | new StringValueExpression("6P%"))
                },
                {
                    // "!!(*gHuQ>a0\\!>\\:\t\\-\\021+o\\]AL2oVmf\\029\\!,*\"Oi\\012j8G]t:JK%H6m>+r{)[5\n6\"*)",
                    (!!(new EndsWithExpression(RawString.From("gHuQ>a0!>:\t-\\021+o\\]AL2oVmf\\029!"))
                    &
                    new ContainsExpression(new TextExpression("Oi\\012j8G]t:JK%H6m>+r{)[5\n6")))).EscapedParseableString,
                    !!(
                        new EndsWithExpression(RawString.From("gHuQ>a0!>:\t-\\021+o\\]AL2oVmf\\029!"))
                        &
                        new ContainsExpression(new TextExpression("Oi\\012j8G]t:JK%H6m>+r{)[5\n6"))
                    )
                }

            };

        [Theory]
        [MemberData(nameof(NotParsingCases))]
        public void Given_NotExpression_as_parseable_string_Not_should_parse_the_input(EscapedString input, NotExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            _outputHelper.WriteLine($"expected : {expected:d}");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input.Value);
            
            _outputHelper.WriteLine(StringifyTokens(tokens));

            // Act
            NotExpression expression = FilterTokenParser.Not.Parse(tokens);
            
            _outputHelper.WriteLine($"Parsed expression : {expression:d}");

            // Assert
            AssertThatShould_parse(expression, expected);
        }

        public static TheoryData<EscapedString, OneOfExpression> OneOfExpressionCases
            => new()
            {
                {
                    EscapedString.From("[Bb]"),
                    new OneOfExpression(new StringValueExpression("B"), new StringValueExpression("b"))
                },

                {
                    EscapedString.From("[Bb]ruce"),
                    new OneOfExpression(new StringValueExpression("Bruce"), new StringValueExpression("bruce"))
                },

                {
                    EscapedString.From("[Bb]at*"),
                    new OneOfExpression(new StartsWithExpression("Bat"), new StartsWithExpression("bat"))
                },

                {
                    EscapedString.From("Ma*[Nn]"),
                    new OneOfExpression(
                        new StartsWithExpression("Ma") & new EndsWithExpression(EscapedString.From("N")),
                        new StartsWithExpression("Ma") & new EndsWithExpression(EscapedString.From("n"))
                    )
                },

                {
                    EscapedString.From("cat[Ww]oman"),
                    new OneOfExpression(new StringValueExpression("catWoman"), new StringValueExpression("catwoman"))
                },

                {
                    EscapedString.From("*Br[Uu]"),
                    new OneOfExpression(new EndsWithExpression(EscapedString.From("BrU")), new EndsWithExpression(EscapedString.From("Bru")))
                },

                {
                    EscapedString.From("Bo[Bb]"),
                    new OneOfExpression(new StringValueExpression("BoB"), new StringValueExpression("Bob"))
                },

                {
                    EscapedString.From("[Bb]*ob"),
                    new OneOfExpression(new StartsWithExpression("B") & new EndsWithExpression(EscapedString.From("ob")),
                                        new StartsWithExpression("b") &  new EndsWithExpression(EscapedString.From("ob")))
                },

                {
                    EscapedString.From("*[Mm]an"),
                    new OneOfExpression(new EndsWithExpression(EscapedString.From("Man")), new EndsWithExpression(EscapedString.From("man")))
                },

                {
                    EscapedString.From("*[Mm]"),
                    new OneOfExpression(new EndsWithExpression(EscapedString.From("M")), new EndsWithExpression(EscapedString.From("m")))
                },

                {
                    EscapedString.From("[a-z]"),
                    new OneOfExpression([ .. GetCharacters('a', 'z').Select(chr => new StringValueExpression(chr.ToString())) ])
                },

                {
                    EscapedString.From("[a-zA-Z0-9]"),
                    new OneOfExpression([.. GetCharacters('a', 'z').Concat(GetCharacters('A', 'Z'))
                                                               .Concat(GetCharacters('0', '9'))
                                                               .Select(chr => new StringValueExpression(chr.ToString()))
                                                               ])
                },

                {
                    EscapedString.From("{Bat|Sup|Wonder}*"),
                    new OneOfExpression(new StartsWithExpression("Bat"),
                                        new StartsWithExpression("Sup"),
                                        new StartsWithExpression("Wonder"))
                },

                {
                    EscapedString.From("[ab][cd]"),
                    new OneOfExpression(new StringValueExpression("ac"),
                                        new StringValueExpression("ad"),
                                        new StringValueExpression("bc"),
                                        new StringValueExpression("bd"))
                },

                {
                    EscapedString.From("{Bat|Sup|Wonder}"),
                    new OneOfExpression(new StringValueExpression("Bat"),
                                        new StringValueExpression("Sup"),
                                        new StringValueExpression("Wonder"))
                },
                {
                    EscapedString.From("*m[ae]n"),
                    new OneOfExpression(new EndsWithExpression(EscapedString.From("man")),
                                        new EndsWithExpression(EscapedString.From("men")))
                }
            };

        private static IEnumerable<char> GetCharacters(char regexStart, char regexEnd)
            => Enumerable.Range(regexStart, regexEnd - regexStart + 1)
                         .Select(ascii => (char)ascii);

        [Theory]
        [MemberData(nameof(OneOfExpressionCases))]
        public void Should_parse_OneOfExpression(EscapedString input, OneOfExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input.Value);

            // Act
            OneOfExpression expression = FilterTokenParser.OneOf.Parse(tokens);

            // Assert
            AssertThatShould_parse(expression, expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_bracket_expression_OneOf_can_parse_input(NonNull<BracketValue> bracketExpression)
        {
            // Arrange
            EscapedString input = bracketExpression.Item.EscapedParseableString;
            _outputHelper.WriteLine($"input : '{input}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(input.Value);

            OneOfExpression expected = bracketExpression.Item switch
            {
                ConstantBracketValue constant => new OneOfExpression([.. constant.Value.Select(chr => new StringValueExpression(chr.ToString()))]),
                RangeBracketValue range => new OneOfExpression([.. Enumerable.Range(range.Start, range.End - range.Start + 1)
                                                         .Select(ascii => (char)ascii)
                                                         .Select(chr => new StringValueExpression(chr.ToString()))]),
                _ => throw new NotSupportedException()
            };

            // Act
            FilterExpression expression = FilterTokenParser.OneOf.Parse(tokens);

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
                TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString.Value);

                // Act
                IntervalExpression expression = FilterTokenParser.Interval.Parse(tokens);

                // Assert
                AssertThatShould_parse(expression, expected);
            });
        }

        public static TheoryData<string, EscapedString, IntervalExpression> IntervalCases
        {
            get
            {
                TheoryData<string, EscapedString, IntervalExpression> cases = [];
                string[] cultures = ["fr-FR", "en-GB", "en-US"];
                foreach (string culture in cultures)
                {
                    cases.Add
                    (
                        culture,
                        EscapedString.From("[5 TO 5["),
                        new IntervalExpression(new BoundaryExpression(new NumericValueExpression("5"), true), new BoundaryExpression(new NumericValueExpression("5"), false))
                    );

                    cases.Add
                    (
                        culture,
                        EscapedString.From("[1993-08-04T00:25:05.155Z TO 1908-06-08T03:18:46.745-09:50["),
                        new IntervalExpression(new BoundaryExpression(new DateTimeExpression(new DateExpression(1993, 8, 4), new TimeExpression(0, 25, 5, 155), OffsetExpression.Zero), true),
                                               new BoundaryExpression(new DateTimeExpression(new DateExpression(1908, 6, 8), new TimeExpression(3, 18, 46, 745), new OffsetExpression(NumericSign.Minus, 9, 50)), false))
                    );

                    cases.Add
                    (
                        culture,
                        EscapedString.From("]* TO 1963-06-03T15:53:44.609Z]"),
                        new IntervalExpression(max: new BoundaryExpression(new DateTimeExpression(new DateExpression(1963, 6, 3), new TimeExpression(15, 53, 44, 609), OffsetExpression.Zero), true))
                    );

                    cases.Add
                    (
                        culture,
                        EscapedString.From("]2E-05 TO 32["),
                        new IntervalExpression(
                            min: new BoundaryExpression(new NumericValueExpression("2E-05"), included: false),
                            max: new BoundaryExpression(new NumericValueExpression("32"), included: false))
                    );
                    cases.Add
                    (
                        culture,
                        EscapedString.From("]-2E-05 TO 32["),
                        new IntervalExpression(
                            min: new BoundaryExpression(new NumericValueExpression("-2E-05"), included: false),
                            max: new BoundaryExpression(new NumericValueExpression("32"), included: false))
                    );
                    cases.Add
                    (
                        culture,
                        EscapedString.From("]+2E-05 TO 32["),
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
        public void Given_interval_as_string_Parser_should_parse_to_IntervalExpression(string culture, EscapedString input, IntervalExpression expected)
        {
            _cultureSwitcher.Run(culture, () =>
            {
                // Arrange
                _outputHelper.WriteLine($"input : '{input}'");
                TokenList<FilterToken> tokens = _tokenizer.Tokenize(input.Value);

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
                TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString.Value);
                _outputHelper.WriteLine($"Tokens : {StringifyTokens(tokens)}");

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
                        new OneOfExpression(new EndsWithExpression(EscapedString.From("Man")),
                                            new EndsWithExpression(EscapedString.From("man")))
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
                    "Firstname=*Br[uU]",
                    (
                        new PropertyName("Firstname"),
                        new OneOfExpression(new EndsWithExpression(EscapedString.From("Bru")),
                                            new EndsWithExpression(EscapedString.From("BrU")))
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
                                                              new EndsWithExpression(EscapedString.From("")))
                                    ),
                                        new OrExpression(new OrExpression(new DateExpression(2003, 5, 27),
                                                                                    new GuidValueExpression("89ae5244-2a98-16cc-6f99-d17658594604")),
                                                              new OrExpression(new StartsWithExpression("s"), new EndsWithExpression(EscapedString.From("0$")))
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
                        !!new NumericValueExpression("5")
                    )
                },
                {
                    @"Value=%f<\,N\:`aAp#*\)",
                    (
                        new PropertyName("Value"),
                        new StartsWithExpression(@"%f<,N:`aAp#") &  new EndsWithExpression(RawString.From(")"))
                    )
                },
                {
                    $"Prop={(new StartsWithExpression(@"1\.\""Foo\!") & new ContainsExpression(@"2\.\""Bar\!")).EscapedParseableString}",
                    (
                        new PropertyName("Prop"),
                        new StartsWithExpression(@"1\.\""Foo\!") & new ContainsExpression(@"2\.\""Bar\!")
                    )
                },
                {
                    $"Prop={(new EndsWithExpression(EscapedString.From("Foo")) & new ContainsExpression("Bar")).EscapedParseableString}",
                    (
                        new PropertyName("Prop"),
                        new EndsWithExpression(EscapedString.From("Foo")) & new ContainsExpression("Bar")
                    )
                }
                ,
                {
                    $"Prop={(new ContainsExpression("Foo") & new EndsWithExpression(EscapedString.From("Bar"))).EscapedParseableString}",
                    (
                        new PropertyName("Prop"),
                        new ContainsExpression("Foo") & new EndsWithExpression(EscapedString.From("Bar"))
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
            using AssertionScope _ = new();
            prop.Should()
                .NotBeSameAs(expected.prop).And
                .Be(expected.prop);

            expression.Should()
                .NotBeSameAs(expected.expression).And
                .Be(expected.expression);
        }

        public static TheoryData<string, Expression<Func<IReadOnlyList<(PropertyName prop, FilterExpression expression)>, bool>>> CriteriaCases
        {
            get
            {
                TheoryData<string, Expression<Func<IReadOnlyList<(PropertyName prop, FilterExpression expression)>, bool>>> cases = new()
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
        public void Should_parse_Criteria(string input, Expression<Func<IReadOnlyList<(PropertyName prop, FilterExpression expression)>, bool>> expectation)
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
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString.Value);

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
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString.Value);

            // Act
            DateExpression actual = FilterTokenParser.Date.Parse(tokens);

            // Assert
            AssertThatShould_parse(actual, expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)], Replay = "(2835403711024236146,16840947086399937335)")]
        public void Should_parse_Groups(GroupExpression expected)
        {
            // Arrange
            _outputHelper.WriteLine($"expected : {expected:f}");
            _outputHelper.WriteLine($"input : '{expected.EscapedParseableString}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString.Value);
            _outputHelper.WriteLine($"Tokens : {StringifyTokens(tokens)}");

            // Act
            GroupExpression actual = FilterTokenParser.Group.Parse(tokens);

            // Assert
            _outputHelper.WriteLine($"actual : {actual:d}");
            _outputHelper.WriteLine($"actual : {actual.EscapedParseableString}");
            AssertThatShould_parse(actual, expected);
        }

        public static TheoryData<EscapedString, GroupExpression> ParseGroupCases
        {
            get
            {
                TheoryData<EscapedString, GroupExpression> cases = new ()
                {
                    {
                        new GroupExpression(new DateTimeExpression(new DateExpression(2090, 10, 10), new TimeExpression(3, 0, 40, 583), OffsetExpression.Zero)).EscapedParseableString,
                        new GroupExpression(new DateTimeExpression(new DateExpression(2090, 10, 10), new TimeExpression(3, 0, 40, 583), OffsetExpression.Zero))
                    },
                    {
                        new GroupExpression(new DateTimeExpression(new DateExpression(2010, 06, 02), new TimeExpression(23, 45, 54, 331), OffsetExpression.Zero)).EscapedParseableString,
                        new GroupExpression(new DateTimeExpression(new DateExpression(2010, 06, 02), new TimeExpression(23, 45, 54, 331), OffsetExpression.Zero))
                    },
                    {
                        EscapedString.From("((((19:53:05.221))))"),
                        new GroupExpression(new GroupExpression(new GroupExpression(new GroupExpression(new TimeExpression(19, 53, 5, 221)))))
                    }
                };
                
                return cases;
            }
        }
        

        [Theory]
        [MemberData(nameof(ParseGroupCases))]
        public void Given_GroupExpression_EscapedParseableString_as_input_Parser_should_return_GroupExpression_that_is_equivalent_to_input(EscapedString escapedParseableString, GroupExpression expected)
        {
            _outputHelper.WriteLine($"input : '{escapedParseableString}'");
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(escapedParseableString.Value);

            // Act
            GroupExpression actual = FilterTokenParser.Group.Parse(tokens);

            // Assert
            AssertThatShould_parse(actual, expected);
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
                TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString.Value);

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
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.Item.EscapedParseableString.Value);

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
            TokenList<FilterToken> tokens = _tokenizer.Tokenize(expected.EscapedParseableString.Value);

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

        /// <summary>
        /// Asserts that <paramref name="actual"/> and <paramref name="expected"/> are
        /// <list type="bullet">
        ///     <item>not the same :</item>
        ///     <item>equal</item>
        /// </list> 
        /// </summary>
        /// <param name="actual">The expression onto which to make assertions.</param>
        /// <param name="expected">The reference expression</param>
        /// <param name="reason">a string that will be displayed if the assertion fails</param>
        private static void AssertThatShould_parse(FilterExpression actual, FilterExpression expected, string reason = "")
        {
            using AssertionScope _ = new ();
            actual.EscapedParseableString.Should().Be(expected.EscapedParseableString);
            actual.Should()
                .NotBeSameAs(expected).And
                .Be(expected, reason);
        }

        private static void AssertThatParseExceptionIsThrown(Action action, string reason)
        {
            // Act
            action.Should().ThrowExactly<ParseException>(reason);
        }
    }
}

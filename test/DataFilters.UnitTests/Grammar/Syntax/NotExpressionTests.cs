using DataFilters.ValueObjects;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    using System;
    using DataFilters.Grammar.Parsing;
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;
    using FluentAssertions;
    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;
    using Superpower;
    using Superpower.Model;
    using Xunit;

    public class NotExpressionTests
    {
        [Fact]
        public void IsFilterExpression() => typeof(NotExpression).Should()
                                                                 .BeAssignableTo<FilterExpression>().And
                                                                 .Implement<IEquatable<NotExpression>>().And
                                                                 .HaveConstructor(new[] { typeof(FilterExpression) }).And
                                                                 .HaveProperty<FilterExpression>("Expression");

        [Fact]
        public void Ctor_should_throws_ArgumentNullException_when_argument_is_null()
        {
            // Act
            Action action = () => _ = new NotExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(NotExpression)}'s constructor cannot be null");
        }

        public static TheoryData<string, NotExpression> ParsingCases
            => new()
            {
                { "!!5", new NotExpression(new NotExpression(new NumericValueExpression("5"))) }
            };

        [Theory]
        [MemberData(nameof(ParsingCases))]
        public void Should_parse_to_NotExpression(string input, NotExpression expected)
        {
            // Arrange
            FilterTokenizer tokenizer = new();
            TokenList<FilterToken> tokens = tokenizer.Tokenize(input);

            // Act
            NotExpression actual = FilterTokenParser.Not.Parse(tokens);

            // Assert
            actual.Should().Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_NotExpression_GetComplexity_should_return_same_complexity_as_embedded_expression(NonNull<NotExpression> notExpression)
            => notExpression.Item.Complexity.Should().Be(notExpression.Item.Expression.Complexity);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_NotExpression_Equals_should_be_reflexive(NonNull<NotExpression> notExpression)
            => notExpression.Item.Should().Be(notExpression.Item);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_NotExpression_and_a_filter_expression_that_is_not_null_Equals_should_be_symetric(NonNull<NotExpression> notExpression, NonNull<FilterExpression> filterExpression)
            => notExpression.Item.Equals(filterExpression.Item).Should().Be(filterExpression.Item.Equals(notExpression.Item));

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_two_instances_holding_same_Expressions_Equals_should_return_true(NonNull<FilterExpression> expression)
        {
            // Arrange
            NotExpression first = new(expression.Item);
            NotExpression other = new(expression.Item);

            // Act
            bool actual = first.Equals(other);
            int firstHashCode = first.GetHashCode();
            int otherHashCode = other.GetHashCode();

            // Assert
            actual.Should().BeTrue();
            firstHashCode.Should().Be(otherHashCode);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_argument_is_AndExpression_Constructor_should_wrap_it_inside_a_GroupExpression(NonNull<BinaryFilterExpression> expression)
        {
            // Act
            NotExpression not = new(expression.Item);

            // Assert
            not.Expression.Should()
                          .BeOfType<GroupExpression>("the parameter is a BinaryFilterExpression");
        }

        public static TheoryData<NotExpression, EscapedString> EscapedParseableStringCases
            => new()
            {
                {
                    new NotExpression(
                        new OrExpression(new DateTimeExpression(new TimeExpression(2, 53, 39, 827)),
                                                  new DateTimeExpression(new DateExpression(1901,06,17),
                                                                         new TimeExpression(09, 13,44, 17),
                                                                         OffsetExpression.Zero))
                        ),

                    EscapedString.From("!(T02:53:39.827|1901-06-17T09:13:44.17Z)")
                },
                {
                    new NotExpression(
                       new OrExpression(new TimeExpression(2, 53, 39, 827),
                                                  new DateTimeExpression(new DateExpression(1901,06,17),
                                                                         new TimeExpression(09, 13,44, 17),
                                                                         OffsetExpression.Zero))
                        ),

                    EscapedString.From("!(02:53:39.827|1901-06-17T09:13:44.17Z)")
                }
            };

        [Theory]
        [MemberData(nameof(EscapedParseableStringCases))]
        public void Given_NotExpression_EscapedParseableString_should_match_expected(NotExpression expression, EscapedString expected)
        {
            // Act
            EscapedString actual = expression.EscapedParseableString;

            // Assert
            actual.Should()
                  .Be(expected);
        }

        public static TheoryData<NotExpression, object, bool, string> EqualsCases
            => new()
            {
                {
                     new NotExpression(
                        new OrExpression(new DateTimeExpression(new TimeExpression(2, 53, 39, 827)),
                                                  new DateTimeExpression(new DateExpression(1901,06,17),
                                                                         new TimeExpression(09, 13,44, 17),
                                                                         OffsetExpression.Zero))
                        ),
                      new NotExpression(
                        new OrExpression(new TimeExpression(2, 53, 39, 827),
                                                  new DateTimeExpression(new DateExpression(1901,06,17),
                                                                         new TimeExpression(09, 13,44, 17),
                                                                         OffsetExpression.Zero))
                        ),
                    true,
                    $"{nameof(DateTimeExpression.Date)} and {nameof(DateTimeExpression.Date)} are null and TimeExpression are equal"
                },
                {
                    new NotExpression(new NotExpression(new NumericValueExpression("5"))),
                    new NotExpression(new NotExpression(new NumericValueExpression("5"))),
                    true,
                    $"Two {nameof(NotExpression)} instances that contains equal inner expressions"
                }
            };

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void Equals_should_work_as_expected(NotExpression expression, object obj, bool expected, string reason)
        {
            // Act
            bool actual = expression.Equals(obj);

            // Assert
            actual.Should()
                  .Be(expected, reason);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_commutative(NonNull<NotExpression> first, FilterExpression second)
            => (first.Item.Equals(second) == second.Equals(first.Item)).ToProperty();

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_reflexive(NonNull<NotExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty();

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_symetric(NonNull<NotExpression> expression, NonNull<FilterExpression> otherExpression)
            => expression.Item.Equals(otherExpression.Item).Should().Be(otherExpression.Item.Equals(expression.Item));

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_NotExpression_Simplify_should_return_the_expected_expression(FilterExpression innerExpression)
        {
            // Act
            NotExpression notExpression = new(innerExpression);
            double complexityBeforeSimplification = notExpression.Complexity;

            // Act
            FilterExpression simplifiedExpression = notExpression.Simplify();

            // Assert
            notExpression.IsEquivalentTo(simplifiedExpression);
        }

    }
}

namespace DataFilters.UnitTests.Grammar.Syntax
{
    using System;
    using System.Collections.Generic;
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;
    using FluentAssertions;
    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;
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
            Action action = () => new NotExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(NotExpression)}'s constructor cannot be null");
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

        public static IEnumerable<object[]> EscapedParseableStringCases
        {
            get
            {
                yield return new object[]
                {
                    new NotExpression(
                        new OrExpression(new DateTimeExpression(new TimeExpression(2, 53, 39, 827)),
                                                  new DateTimeExpression(new DateExpression(1901,06,17),
                                                                         new TimeExpression(09, 13,44, 17),
                                                                         OffsetExpression.Zero))
                        ),

                    "!(T02:53:39.827|1901-06-17T09:13:44.17Z)"
                };

                yield return new object[]
                {
                    new NotExpression(
                       new OrExpression(new TimeExpression(2, 53, 39, 827),
                                                  new DateTimeExpression(new DateExpression(1901,06,17),
                                                                         new TimeExpression(09, 13,44, 17),
                                                                         OffsetExpression.Zero))
                        ),

                    "!(02:53:39.827|1901-06-17T09:13:44.17Z)"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EscapedParseableStringCases))]
        public void Given_NotExpression_EscapedParseableString_should_match_expected(NotExpression expression, string expected)
        {
            // Act
            string actual = expression.EscapedParseableString;

            // Assert
            actual.Should()
                  .Be(expected);
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
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
                };
            }
        }

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
    }
}

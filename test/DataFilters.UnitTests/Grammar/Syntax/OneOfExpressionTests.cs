namespace DataFilters.UnitTests.Grammar.Syntax
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;
    using FluentAssertions;
    using FsCheck;
    using FsCheck.Xunit;
    using Xunit;
    using Xunit.Abstractions;

    public class OneOfExpressionTests(ITestOutputHelper outputHelper)
    {
        [Fact]
        public void IsFilterExpression() => typeof(OneOfExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<OneOfExpression>>().And
            .HaveConstructor(new[] { typeof(FilterExpression[]) }).And
            .HaveProperty<IReadOnlyCollection<FilterExpression>>("Values");

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => _ = new OneOfExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter {nameof(OneOfExpression)}'s constructor cannot be null");
        }

        [Fact]
        public void Constructor_throws_InvalidOperationException_when_values_is_empty()
        {
            // Act
            Action ctorWithEmptyArray = () => _ = new OneOfExpression(Array.Empty<FilterExpression>());

            // Assert
            ctorWithEmptyArray.Should()
                              .ThrowExactly<InvalidOperationException>($"The parameter {nameof(OneOfExpression)}'s constructor cannot be a empty array");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                    true,
                    "comparing two different instances with same data in same order"
                };

                yield return new object[]
                {
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                    new OneOfExpression(new StringValueExpression("prop2"), new StringValueExpression("prop1")),
                    false,
                    "comparing two different instances with same data but the order does not matter"
                };

                yield return new object[]
                {
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop3")),
                    false,
                    "comparing two different instances with different data"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(OneOfExpression first, object other, bool expected, string reason)
        {
            outputHelper.WriteLine($"First instance : {first}");
            outputHelper.WriteLine($"Second instance : {other}");

            // Act
            bool actual = first.Equals(other);
            int actualHashCode = first.GetHashCode();

            // Assert
            actual.Should()
                .Be(expected, reason);
            if (expected)
            {
                actualHashCode.Should()
                    .Be(other?.GetHashCode(), reason);
            }
            else
            {
                actualHashCode.Should()
                    .NotBe(other?.GetHashCode(), reason);
            }
        }

        public static IEnumerable<object[]> IsEquivalentToCases
        {
            get
            {
                yield return new object[]
                {
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                    true,
                    "comparing two different instances with same data in same order"
                };

                yield return new object[]
                {
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                    new OneOfExpression(new StringValueExpression("prop2"), new StringValueExpression("prop1")),
                    true,
                    "comparing two different instances with same data but the order does not matter"
                };

                yield return new object[]
                {
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop3")),
                    false,
                    "comparing two different instances with different data"
                };

                yield return new object[]
                {
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2"), new StringValueExpression("prop3")),
                    false,
                    "the other instance contains all data of the first instance and one item that is not in the current instance"
                };

                yield return new object[]
                {
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                    true,
                    $"a {nameof(OneOfExpression)} instance that holds duplicates is equivalent a {nameof(OneOfExpression)} with no duplicate"
                };

                yield return new object[]
                {
                    new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                    new OrExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                    true,
                    $"a {nameof(OneOfExpression)} instance that holds two distinct value is equivalent to an {nameof(OrExpression)} with the same values"
                };

                yield return new object[]
                {
                    new OneOfExpression(new IntervalExpression(new BoundaryExpression(new NumericValueExpression("-1"), true),
                                                                new BoundaryExpression(new NumericValueExpression("-1"), true)),
                                               new IntervalExpression(new BoundaryExpression(new NumericValueExpression("-1"), true),
                                                                new BoundaryExpression(new NumericValueExpression("-1"), true))),
                    new NumericValueExpression("-1"),
                    true,
                    $"a {nameof(OneOfExpression)} instance that holds two distinct value is equivalent to its simplified version"
                };
            }
        }

        [Theory]
        [MemberData(nameof(IsEquivalentToCases))]
        public void Implements_IsEquivalentTo_Properly(OneOfExpression first, FilterExpression other, bool expected, string reason)
        {
            // Act
            bool actual = first.IsEquivalentTo(other);

            // Assert
            actual.Should()
                  .Be(expected, reason);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_ConstantExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(StringValueExpression filterExpression, PositiveInt n)
            => Given_OneOfExpression_contains_the_same_epxression_repeated_many_time_When_comparing_to_that_expression_IsEquivalentTo_should_return_true(filterExpression, n.Item);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_DateExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(DateExpression filterExpression, PositiveInt n)
            => Given_OneOfExpression_contains_the_same_epxression_repeated_many_time_When_comparing_to_that_expression_IsEquivalentTo_should_return_true(filterExpression, n.Item);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_DateTimeExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(DateTimeExpression filterExpression, PositiveInt n)
            => Given_OneOfExpression_contains_the_same_epxression_repeated_many_time_When_comparing_to_that_expression_IsEquivalentTo_should_return_true(filterExpression, n.Item);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_AsteriskExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(PositiveInt n)
            => Given_OneOfExpression_contains_the_same_epxression_repeated_many_time_When_comparing_to_that_expression_IsEquivalentTo_should_return_true(AsteriskExpression.Instance, n.Item);

        private static void Given_OneOfExpression_contains_the_same_epxression_repeated_many_time_When_comparing_to_that_expression_IsEquivalentTo_should_return_true(FilterExpression filterExpression, int n)
        {
            // Arrange
            IEnumerable<FilterExpression> filterExpressions = Enumerable.Repeat(filterExpression, n);

            OneOfExpression oneOfExpression = new(filterExpressions.ToArray());

            // Act
            bool actual = oneOfExpression.IsEquivalentTo(filterExpression);

            // Assert
            actual.Should().BeTrue();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_OneOfExpression_Complexity_should_return_sum_of_inner_expressions(NonEmptyArray<FilterExpression> expressions)
        {
            // Arrange
            OneOfExpression oneOfExpression = new(expressions.Item);
            double expected = oneOfExpression.Values.Sum(expr => expr.Complexity);

            // Act
            double actual = oneOfExpression.Complexity;

            // Assert
            actual.Should().Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_OneOfExpression_with_same_values_repetead_more_than_once_Simplify_should_filter_out_duplicated_values(NonEmptyArray<FilterExpression> expressions)
        {
            // Arrange
            OneOfExpression oneOf = new(expressions.Item);

            double complexityBeforeCallingSimplify = oneOf.Complexity;

            // Act
            FilterExpression simplifiedExpression = oneOf.Simplify();

            // Assert
            simplifiedExpression.Complexity.Should().BeLessThanOrEqualTo(complexityBeforeCallingSimplify);
        }

        public static IEnumerable<object[]> SimplifyCases
        {
            get
            {
                yield return new object[]
                {
                    new OneOfExpression(new StringValueExpression("val1"), new StringValueExpression("val2")),
                    new OrExpression(new StringValueExpression("val1"), new StringValueExpression("val2"))
                };

                yield return new object[]
                {
                    new OneOfExpression(new StringValueExpression("val1"), new StringValueExpression("val1")),
                    new StringValueExpression("val1")
                };

                yield return new object[]
                {
                    new OneOfExpression(new StringValueExpression("val1"), new OrExpression(new StringValueExpression("val1"), new StringValueExpression("val1"))),
                    new StringValueExpression("val1")
                };

                yield return new object[]
                {
                    new OneOfExpression(new IntervalExpression(new BoundaryExpression(new NumericValueExpression("-1"), true),
                                                                new BoundaryExpression(new NumericValueExpression("-1"), true)),
                                               new IntervalExpression(new BoundaryExpression(new NumericValueExpression("-1"), true),
                                                                new BoundaryExpression(new NumericValueExpression("-1"), true))),
                    new NumericValueExpression("-1"),
                };
            }
        }

        [Theory]
        [MemberData(nameof(SimplifyCases))]
        public void Given_OneOfExpression_Simplify_should_output_expected_expression(OneOfExpression input, FilterExpression expected)
        {
            // Act
            FilterExpression actual = input.Simplify();

            // Assert
            actual.Should().Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_OneOfExpression_instance_which_contains_only_one_expression_Simplify_should_return_an_expression_that_is_requivalent_to_the_inner_expression(NonNull<FilterExpression> expression)
        {
            // Arrange
            outputHelper.WriteLine($"input : {expression.Item}");
            OneOfExpression oneOf = new(expression.Item);

            // Act
            FilterExpression simplified = oneOf.Simplify();

            outputHelper.WriteLine($"Simplified : {simplified}");

            // Assert
            simplified.IsEquivalentTo(expression.Item)
                      .Should()
                      .BeTrue();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_OneOfExpression_Simplify_should_return_an_expression_that_cannot_be_further_simplified(PositiveInt count, NonNull<FilterExpression> expression)
        {
            // Arrange
            FilterExpression[] expressions = Enumerable.Repeat(expression.Item, count.Item + 2)
                                                       .ToArray();

            OneOfExpression oneOfExpression = new(expressions);

            double complexityBeforeFirstSimplification = oneOfExpression.Complexity;

            // Act
            FilterExpression simplified = oneOfExpression.Simplify();

            // Assert
            simplified.Complexity.Should()
                                 .BeLessThan(complexityBeforeFirstSimplification, "The expression must be simpler");
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_OneExpression_that_contains_inner_OneOfExpressions_Simplify_should_flatten_them(NonEmptyArray<FilterExpression> first, NonEmptyArray<FilterExpression> second, NonEmptyArray<FilterExpression> third)
        {
            // Arrange
            FilterExpression expected = new OneOfExpression([.. first.Item, .. second.Item, .. third.Item]).Simplify();

            OneOfExpression initial = new(new OneOfExpression(first.Item),
                                          new OneOfExpression(second.Item),
                                          new OneOfExpression(third.Item));

            // Act
            FilterExpression actual = initial.Simplify();

            // Assert
            actual.Should()
                  .Be(expected);
        }
    }
}


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

namespace DataFilters.UnitTests.Grammar.Syntax;
public class OneOfExpressionTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void IsFilterExpression() => typeof(OneOfExpression).Should()
        .BeAssignableTo<FilterExpression>().And
        .Implement<IEquatable<OneOfExpression>>().And
        .HaveConstructor(new[] { typeof(FilterExpression[]) }).And
        .HaveProperty<IReadOnlyList<FilterExpression>>(nameof(OneOfExpression.Values));

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
        Action ctorWithEmptyArray = () => _ = new OneOfExpression([]);

        // Assert
        ctorWithEmptyArray.Should()
            .ThrowExactly<InvalidOperationException>($"The parameter {nameof(OneOfExpression)}'s constructor cannot be a empty array");
    }

    public static TheoryData<OneOfExpression, object, bool, string> EqualsCases
        => new()
        {
            {
                new OneOfExpression(new StringValueExpression("prop1")),
                new OneOfExpression(new StringValueExpression("prop1")),
                true,
                "comparing two different instances with same data in same order"
            },
            {
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                true,
                "comparing two different instances with same data in same order"
            },
            {
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                new OneOfExpression(new StringValueExpression("prop2"), new StringValueExpression("prop1")),
                false,
                "comparing two different instances with same data but the order does not matter"
            },
            {
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop3")),
                false,
                "comparing two different instances with different data"
            }
        };

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

        object _ = expected switch
        {
            true => actualHashCode.Should()
                .Be(other?.GetHashCode(), reason),
            _ => true
        };
    }

    public static TheoryData<OneOfExpression, FilterExpression, bool, string> IsEquivalentToCases
        => new()
        {
            {
                new OneOfExpression(new StringValueExpression("prop1")),
                new OneOfExpression(new StringValueExpression("prop1")),
                true,
                "comparing two different instances with only one value"
            },
            {
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                true,
                "comparing two different instances with same data in same order"
            },
            {
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                new OneOfExpression(new StringValueExpression("prop2"), new StringValueExpression("prop1")),
                true,
                "comparing two different instances with same data but the order does not matter"
            },
            {
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop3")),
                false,
                "comparing two different instances with different data"
            },
            {
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2"), new StringValueExpression("prop3")),
                false,
                "the other instance contains all data of the first instance and one item that is not in the current instance"
            },
            {
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                true,
                $"a {nameof(OneOfExpression)} instance that holds duplicates is equivalent a {nameof(OneOfExpression)} with no duplicate"
            },
            {
                new OneOfExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                new OrExpression(new StringValueExpression("prop1"), new StringValueExpression("prop2")),
                true,
                $"a {nameof(OneOfExpression)} instance that holds two distinct value is equivalent to an {nameof(OrExpression)} with the same values"
            },
            {
                new OneOfExpression(new IntervalExpression(new BoundaryExpression(new NumericValueExpression("-1"), true),
                        new BoundaryExpression(new NumericValueExpression("-1"), true)),
                    new IntervalExpression(new BoundaryExpression(new NumericValueExpression("-1"), true),
                        new BoundaryExpression(new NumericValueExpression("-1"), true))),
                new NumericValueExpression("-1"),
                true,
                $"a {nameof(OneOfExpression)} instance that holds two distinct value is equivalent to its simplified version"
            }
        };

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
        => Given_OneOfExpression_contains_the_same_expression_repeated_many_time_When_comparing_to_that_expression_IsEquivalentTo_should_return_true(filterExpression, n.Item);

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_DateExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(DateExpression filterExpression, PositiveInt n)
        => Given_OneOfExpression_contains_the_same_expression_repeated_many_time_When_comparing_to_that_expression_IsEquivalentTo_should_return_true(filterExpression, n.Item);

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_DateTimeExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(DateTimeExpression filterExpression, PositiveInt n)
        => Given_OneOfExpression_contains_the_same_expression_repeated_many_time_When_comparing_to_that_expression_IsEquivalentTo_should_return_true(filterExpression, n.Item);

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_AsteriskExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(PositiveInt n)
        => Given_OneOfExpression_contains_the_same_expression_repeated_many_time_When_comparing_to_that_expression_IsEquivalentTo_should_return_true(AsteriskExpression.Instance, n.Item);

    private static void Given_OneOfExpression_contains_the_same_expression_repeated_many_time_When_comparing_to_that_expression_IsEquivalentTo_should_return_true(FilterExpression filterExpression, int n)
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
    public void Given_OneOfExpression_with_same_values_repeated_more_than_once_Simplify_should_filter_out_duplicated_values(NonEmptyArray<FilterExpression> expressions)
    {
        // Arrange
        OneOfExpression oneOf = new(expressions.Item);

        double complexityBeforeCallingSimplify = oneOf.Complexity;

        // Act
        FilterExpression simplifiedExpression = oneOf.Simplify();

        // Assert
        simplifiedExpression.Complexity.Should().BeLessThanOrEqualTo(complexityBeforeCallingSimplify);
    }

    public static TheoryData<OneOfExpression, FilterExpression> SimplifyCases
        => new()
        {
            {
                new OneOfExpression(new StringValueExpression("val1"), new StringValueExpression("val2")),
                new OrExpression(new StringValueExpression("val1"), new StringValueExpression("val2"))
            },
            {
                new OneOfExpression(new StringValueExpression("val1"), new StringValueExpression("val1")),
                new StringValueExpression("val1")
            },
            {
                new OneOfExpression(new StringValueExpression("val1"), new OrExpression(new StringValueExpression("val1"), new StringValueExpression("val1"))),
                new StringValueExpression("val1")
            },
            {
                new OneOfExpression(new IntervalExpression(new BoundaryExpression(new NumericValueExpression("-1"), true),
                        new BoundaryExpression(new NumericValueExpression("-1"), true)),
                    new IntervalExpression(new BoundaryExpression(new NumericValueExpression("-1"), true),
                        new BoundaryExpression(new NumericValueExpression("-1"), true))),
                new NumericValueExpression("-1")
            },
            {
                new OneOfExpression(null, new GroupExpression(new AndExpression(new StringValueExpression("a"), new StringValueExpression("b"))), null),
                new AndExpression(new StringValueExpression("a"), new StringValueExpression("b"))
            },
            {
                new OneOfExpression(new ContainsExpression("A"), new ContainsExpression("A"), new ContainsExpression("A")),
                new ContainsExpression("A")
            }
        };

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
    public void Given_OneOfExpression_instance_which_contains_only_one_expression_Simplify_should_return_an_expression_that_is_equivalent_to_the_inner_expression(NonNull<FilterExpression> expression)
    {
        // Arrange
        FilterExpression expected = expression.Item;
        outputHelper.WriteLine($"input : {expected:d}");
        OneOfExpression oneOf = new(expected);

        // Act
        FilterExpression actual = oneOf.Simplify();

        outputHelper.WriteLine($"Actual : {actual:d}");

        // Assert
        actual.IsEquivalentTo(expected)
            .Should()
            .BeTrue();
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_OneOfExpression_Simplify_should_return_an_expression_that_cannot_be_further_simplified(PositiveInt count, NonNull<FilterExpression> expression)
    {
        // Arrange
        FilterExpression[] expressions = [ .. Enumerable.Repeat(expression.Item, count.Item + 2) ];

        OneOfExpression oneOfExpression = new(expressions);

        double complexityBeforeFirstSimplification = oneOfExpression.Complexity;

        // Act
        FilterExpression simplified = oneOfExpression.Simplify();

        // Assert
        simplified.Complexity.Should()
            .BeLessThan(complexityBeforeFirstSimplification, "The expression must be simpler");
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_OneExpression_that_contains_inner_OneOfExpressions_Simplify_should_flatten_them(NonEmptyArray<FilterExpression> oneOfExpressionGenerator)
    {
        // Arrange
        FilterExpression[] oneOfExpressions = oneOfExpressionGenerator.Item;
        FilterExpression expected = new OneOfExpression([.. oneOfExpressions]).Simplify();
        outputHelper.WriteLine($"expected  (debug): {expected:d}");
        outputHelper.WriteLine($"expected : {expected}");

        OneOfExpression initial = new([.. oneOfExpressions]);
        outputHelper.WriteLine($"initial : {initial:d}");

        // Act
        FilterExpression actual = initial.Simplify();
        outputHelper.WriteLine($"actual (debug): {actual:d}");
        outputHelper.WriteLine($"actual : {actual}");

        // Assert
        actual.Should()
            .Be(expected);
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_several_expressions_When_wrapped_in_OneOfExpression_Then_EscapedParseableString_should_have_expected_value(NonEmptyArray<FilterExpression> expressionGenerators)
    {
        // Arrange
        FilterExpression[] innerExpressions = [.. expressionGenerators.Item];
        OneOfExpression expression = new(innerExpressions);
        string expected = $"{{{string.Join("|", innerExpressions.Select(expr => expr.EscapedParseableString))}}}";

        // Act
        string actual = expression.EscapedParseableString;

        // Assert
        actual.Should().Be(expected);
    }
}
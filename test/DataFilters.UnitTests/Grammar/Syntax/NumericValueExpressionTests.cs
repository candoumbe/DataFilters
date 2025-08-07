using System;
using Candoumbe.MiscUtilities.Comparers;
using DataFilters.Grammar.Syntax;
using DataFilters.UnitTests.Helpers;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace DataFilters.UnitTests.Grammar.Syntax;
[UnitTest]
public class NumericValueExpressionTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void IsFilterExpression() => typeof(NumericValueExpression).Should()
        .BeAssignableTo<FilterExpression>().And
        .Implement<IEquatable<NumericValueExpression>>().And
        .Implement<IBoundaryExpression>();

    [Fact]
    public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
    {
        // Act
        Action action = () => _ = new NumericValueExpression(null);

        // Assert
        action.Should()
            .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(NumericValueExpression)}'s constructor cannot be null");
    }

    [Fact]
    public void Ctor_Throws_ArgumentOutOfRangeException_When_Argument_Is_Empty()
    {
        // Act
        Action action = () => _ = new NumericValueExpression(string.Empty);

        // Assert
        action.Should()
            .ThrowExactly<ArgumentOutOfRangeException>($"The parameter of  {nameof(StringValueExpression)}'s constructor cannot be empty");
    }

    [Fact]
    public void Given_parameter_is_whitespace_Ctor_should_not_throw()
    {
        // Act
        Action action = () => _ = new NumericValueExpression(" ");

        // Assert
        action.Should()
            .NotThrow($"The parameter of {nameof(StringValueExpression)}'s constructor can be whitespace");
    }

    [Property]
    public void Two_instances_are_equals_when_holding_same_values(NonEmptyString value)
    {
        // Arrange
        NumericValueExpression first = new(value.Item);
        NumericValueExpression other = new(value.Item);

        // Act
        bool actual = first.Equals(other);

        // Assert
        actual.Should().BeTrue();
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_is_commutative(NumericValueExpression first, NumericValueExpression second)
        => first.Equals(second).Should().Be(second.Equals(first));

    [Property]
    public void Given_two_NumericValueExpressions_Equals_should_depends_on_string_input_only(NonWhiteSpaceString input)
    {
        // Arrange
        NumericValueExpression first = new(input.Get);
        NumericValueExpression second = new(input.Get);

        // Act
        first.Equals(second).Should().Be(first.Value.Equals(second.Value, CharComparer.Ordinal));
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_NumericValueExpression_GetComplexity_should_return_1(NumericValueExpression constant)
        => constant.Complexity.Should().Be(1);

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_IntervalExpression_with_min_equals_max_is_equivalent_should_return_true(NumericValueExpression input)
    {
        // Arrange
        IntervalExpression interval = new(new BoundaryExpression(input, true),
                                          new BoundaryExpression(input, true));

        // Act
        bool actual = input.IsEquivalentTo(interval);

        // Assert
        actual.Should()
            .BeTrue();
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_should_be_commutative(NonNull<NumericValueExpression> first, FilterExpression second)
        => first.Item.Equals(second).Should().Be(second.Equals(first.Item));

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_should_be_reflexive(NonNull<NumericValueExpression> expression)
        => expression.Item.Should().Be(expression.Item);

    [Property(Arbitrary = [typeof(ExpressionsGenerators)], Replay = "(2263177707555740354,6550430739131839265)")]
    public void Equals_should_be_symmetric(NonNull<NumericValueExpression> expression, NonNull<FilterExpression> otherExpression)
    {
        // Arrange
        NumericValueExpression left = expression.Item;
        FilterExpression other = otherExpression.Item;

        outputHelper.WriteLine($"Left is {left:d}");
        outputHelper.WriteLine($"other is {other:d}");

        // Assert
        left.Equals(other).Should().Be(other.Equals(left));
    }

    public static TheoryData<NumericValueExpression, object, bool, string> EqualsCases
        => new()
        {
            { new NumericValueExpression("0"), new NumericValueExpression("0"), true, $"Left and right are both {nameof(NumericValueExpression)}s with exact same values" },
            { new NumericValueExpression("0"), new StringValueExpression("0"), true, $"Right is {nameof(StringValueExpression)} and hold the same value as current instance" },
            { new NumericValueExpression("+0"), new NumericValueExpression("0"), false, $"Left and right values are not the same numbers" },
            {
                new NumericValueExpression("1"), new GuidValueExpression("13a26ddf-33ce-f600-223f-09d5193fe9bf"), false, "NumericValueExpression is not a Guid"
            },
        };

    [Theory]
    [MemberData(nameof(EqualsCases))]
    public void Given_Left_operand_and_Right_operand_When_calling_Equals_Then_should_have_the_expected_result(NumericValueExpression left, object right, bool expected, string reason)
    {
        // Act
        bool actual = left.Equals(right);

        // Assert
        actual.Should().Be(expected, reason);
    }
}
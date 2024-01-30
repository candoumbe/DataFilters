namespace DataFilters.UnitTests.Grammar.Syntax;

using System;
using System.Linq;
using DataFilters.Grammar.Syntax;
using DataFilters.UnitTests.Helpers;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

public class ConstantBracketValueTests
{
    [Property]
    public void Value_should_be_set_by_the_parameter_of_the_constructor(NonNull<string> input)
    {
        // Act
        ConstantBracketValue constantBracketValue = new(input.Item);

        // Assert
        constantBracketValue.Value.Should()
                                  .Be(input.Item);
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_RangeBracketValue_Equals_should_returns_true_when_ConstantBracketValue_contains_all_letters_of_the_interval(NonNull<RangeBracketValue> rangeBracketValue)
    {
        // Arrange
        ConstantBracketValue constantBracketValue = new(Enumerable.Range(rangeBracketValue.Item.Start, rangeBracketValue.Item.End - rangeBracketValue.Item.Start + 1)
                                                                  .Select(ascii => ((char)ascii).ToString())
                                                                  .Aggregate((accumulate, current) => $"{accumulate}{current}"));

        // Act
        bool actual = constantBracketValue.Equals(rangeBracketValue.Item);

        actual.Should().BeTrue($"Range expression : {rangeBracketValue} and Constant expression is {constantBracketValue.Value}");
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_left_and_right_ConstantBracketValues_left_eq_right_should_be_returns_same_value_as_Equals(ConstantBracketValue left, ConstantBracketValue right)
        => (left == right).When(left.Equals(right)).Label($"Left, Right : {(left, right)}");

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_left_and_right_ConstantBracketValues_left_neq_right_should_be_returns_same_value_as_Equals(ConstantBracketValue left, ConstantBracketValue right)
        => (left != right).When(!left.Equals(right)).Label($"Left, Right : {(left, right)}");

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_should_be_reflexive(NonNull<ConstantBracketValue> expression)
    {
        // Act
        bool actual = expression.Item.Equals(expression.Item);

        // Assert
        actual.Should().BeTrue("'equals' implementation is reflexive");
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_should_be_symetric(NonNull<ConstantBracketValue> expression, NonNull<BracketValue> otherExpression)
    {
        // Act
        bool actual = expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item);

        // Assert
        actual.Should().BeTrue("'equals' implementation should be symetric");
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_ConstantBracketValue_Complexity_should_be_equal_to_inner_expression_complexity(ConstantBracketValue value)
    {
        // Arrange
        double expected = 1 + Math.Pow(2, value.Value.Length);

        // Act
        double actual = value.Complexity;

        // Assert
        actual.Should().Be(expected);
    }
}

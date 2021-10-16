namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;

    using System;
    using System.Linq;

    public class ConstantBracketValueTests
    {
        [Property]
        public Property Value_should_be_set_by_the_parameter_of_the_constructor(NonNull<string> input)
        {
            // Act
            ConstantBracketValue constantBracketValue = new(input.Item);

            // Assert
            return (constantBracketValue.Value == input.Item).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_RangeBracketValue_Equals_should_returns_true_when_ConstantBracketValue_contains_all_letters_of_the_interval(NonNull<RangeBracketValue> rangeBracketValue)
        {
            // Arrange
            ConstantBracketValue constantBracketValue = new(Enumerable.Range(rangeBracketValue.Item.Start, rangeBracketValue.Item.End - rangeBracketValue.Item.Start + 1)
                                                                      .Select(ascii => ((char)ascii).ToString())
                                                                      .Aggregate((accumulate, current) => $"{accumulate}{current}"));

            // Act
            bool actual = constantBracketValue.Equals(rangeBracketValue.Item);

            return actual.ToProperty().Label($"Range expression : {rangeBracketValue} and Constant expression is {constantBracketValue.Value}");
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_left_and_right_ConstantBracketValues_left_eq_right_should_be_returns_same_value_as_Equals(ConstantBracketValue left, ConstantBracketValue right)
            => (left == right).When(left.Equals(right)).Label($"Left, Right : {(left, right)}");

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_left_and_right_ConstantBracketValues_left_neq_right_should_be_returns_same_value_as_Equals(ConstantBracketValue left, ConstantBracketValue right)
            => (left != right).When(!left.Equals(right)).Label($"Left, Right : {(left, right)}");

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_reflexive(NonNull<ConstantBracketValue> expression)
            => expression.Item.Equals(expression.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_symetric(NonNull<ConstantBracketValue> expression, NonNull<BracketValue> otherExpression)
            => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_ConstantBracketValue_Complexity_should_be_equal_to_inner_expression_complexity(ConstantBracketValue value)
            => (value.Complexity == 1 + Math.Pow(2, value.Value.Length)).ToProperty();
    }
}

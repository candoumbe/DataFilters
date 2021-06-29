namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Xunit;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Xunit;
    using Xunit.Abstractions;

    public class BracketExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public BracketExpressionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void IsFilterExpression() => typeof(BracketExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<BracketExpression>>().And
            .Implement<IHaveComplexity>().And
            .HaveConstructor(new[] { typeof(BracketValue) }).And
            .HaveProperty<BracketValue>("Value")
            ;

        [Fact]
        public void Ctor_Throws_ArgumentNullException_If_Value_Is_Null()
        {
            // Act
            Action action = () => new BracketExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"{nameof(BracketExpression)}.{nameof(BracketExpression.Value)} cannot be null");
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Two_BracketExpression_instances_built_with_same_input_should_be_equals(NonNull<BracketValue> inputs)
        {
            // Arrange
            BracketExpression first = new(inputs.Item);
            BracketExpression second = new(inputs.Item);

            // Act
            bool actual = first.Equals(second);

            // Assert
            return actual.ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Two_BracketExpression_instances_built_with_different_inputs_should_not_be_equal(NonNull<BracketValue> one, NonNull<BracketValue> two)
        {
            // Arrange
            BracketExpression first = new(one.Item);
            BracketExpression second = new(two.Item);

            // Act
            bool actual = !first.Equals(second);

            // Assert
            return actual.ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Complexity_should_depends_on_input(NonNull<BracketValue> value)
        {
            // Arrange
            BracketExpression bracketExpression = new(value.Item);
            double expected = value.Item switch
            {
                ConstantBracketValue constant => 1.5 * constant.Value.Length,
                RangeBracketValue range => 1.5 * (range.End - range.Start + 1),
                _ => throw new NotSupportedException()
            };

            // Act
            double complexity = bracketExpression.Complexity;

            // Assert
            complexity.Should().Be(expected);
        }

        [Property(Arbitrary = new[] {typeof(ExpressionsGenerators)})]
        public Property Given_BracketRangeValue_IsEquivalentTo_should_be_equivalent_to_many_OrExpression_where_each_expression_contains_one_charater(NonNull<RangeBracketValue> rangeBracketValue)
        {
            // Arrange
            BracketExpression rangeBracketExpression = new(rangeBracketValue.Item);

            OneOfExpression oneOf = new (Enumerable.Range(rangeBracketValue.Item.Start,
                                                          rangeBracketValue.Item.End - rangeBracketValue.Item.Start + 1)
                                                   .Select(ascii => new ConstantValueExpression(((char)ascii).ToString()))
                                                   .ToArray());

            // Act
            bool actual = rangeBracketExpression.IsEquivalentTo(oneOf);

            // Assert
            return actual.ToProperty().Label($"Range expression : {rangeBracketValue.Item}");
        }
    }
}

namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;

    using System;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    public class NumericValueExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public NumericValueExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(NumericValueExpression).Should()
                                                                           .BeAssignableTo<FilterExpression>().And
                                                                           .Implement<IEquatable<NumericValueExpression>>().And
                                                                           .Implement<IBoundaryExpression>();

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => new NumericValueExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(NumericValueExpression)}'s constructor cannot be null");
        }

        [Fact]
        public void Ctor_Throws_ArgumentOutOfRangeException_When_Argument_Is_Empty()
        {
            // Act
            Action action = () => new NumericValueExpression(string.Empty);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>($"The parameter of  {nameof(StringValueExpression)}'s constructor cannot be empty");
        }

        [Fact]
        public void Given_parameter_is_whitespace_Ctor_should_not_throw()
        {
            // Act
            Action action = () => new NumericValueExpression(" ");

            // Assert
            action.Should()
                .NotThrow($"The parameter of {nameof(StringValueExpression)}'s constructor can be whitespace");
        }

        [Property]
        public Property Two_instances_are_equals_when_holding_same_values(NonEmptyString value)
        {
            NumericValueExpression first = new(value.Item);
            NumericValueExpression other = new(value.Item);

            return first.Equals(other).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_is_commutative(NumericValueExpression first, NumericValueExpression second)
            => (first.Equals(second) == second.Equals(first)).ToProperty();
        
        [Property]
        public Property Given_two_NumericValueExpressions_Equals_should_depends_on_string_input_only(NonWhiteSpaceString input)
        {
            // Arrange
            NumericValueExpression first = new(input.Get);
            NumericValueExpression second = new(input.Get);

            // Act
            return (first.Equals(second) == Equals(first.Value, second.Value)).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_NumericValueExpression_GetComplexity_should_return_1(NumericValueExpression constant)
            => (constant.Complexity == 1).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
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

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_commutative(NonNull<TextExpression> first, FilterExpression second)
            => (first.Item.Equals(second) == second.Equals(first.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_reflexive(NonNull<TextExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_symetric(NonNull<TextExpression> expression, NonNull<FilterExpression> otherExpression)
            => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty();

    }
}

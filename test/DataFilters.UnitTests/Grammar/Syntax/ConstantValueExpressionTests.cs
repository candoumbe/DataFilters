namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Xunit;
using OneOf;

    using System;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    public class ConstantValueExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public ConstantValueExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(ConstantValueExpression).Should()
                                                                           .BeAssignableTo<FilterExpression>().And
                                                                           .Implement<IEquatable<ConstantValueExpression>>().And
                                                                           .HaveConstructor(new[] { typeof(OneOf<string, DateTime, DateTimeOffset, short, int, long, Guid, decimal, double, float, bool, byte, char>) }).And
                                                                           .HaveProperty<OneOf<string, DateTime, DateTimeOffset, short, int, long, Guid, decimal, double, float, bool, byte, char>>("Value");

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => new ConstantValueExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(ConstantValueExpression)}'s constructor cannot be null");
        }

        [Fact]
        public void Ctor_Throws_ArgumentOutOfRangeException_When_Argument_Is_Empty()
        {
            // Act
            Action action = () => new ConstantValueExpression(string.Empty);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>($"The parameter of  {nameof(ConstantValueExpression)}'s constructor cannot be empty");
        }

        [Fact]
        public void Given_parameter_is_whitespace_Ctor_should_not_throw()
        {
            // Act
            Action action = () => new ConstantValueExpression(" ");

            // Assert
            action.Should()
                .NotThrow($"The parameter of {nameof(ConstantValueExpression)}'s constructor can be whitespace");
        }

        [Property]
        public Property Two_instances_are_equals_when_holding_same_values(NonEmptyString value)
        {
            ConstantValueExpression first = new(value.Item);
            ConstantValueExpression other = new(value.Item);

            return first.Equals(other).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_is_commutative(ConstantValueExpression first, ConstantValueExpression second) => (first.Equals(second) == second.Equals(first)).ToProperty();

        [Property]
        public Property Value_should_be_DateTimeOffset(DateTimeOffset input)
        {
            ConstantValueExpression expression = new(input);

            return expression.Value.Value.Equals(input).ToProperty()
                    .And(expression.Value.Value.GetType() == input.GetType());
        }

        [Property]
        public Property Value_should_be_Guid(Guid input)
        {
            ConstantValueExpression expression = new(input);

            return expression.Value.Value.Equals(input).ToProperty()
                    .And(expression.Value.Value.GetType() == input.GetType());
        }

        [Property]
        public Property Value_should_be_Bool(bool input)
        {
            ConstantValueExpression expression = new(input);

            return expression.Value.Value.Equals(input).ToProperty()
                    .And(expression.Value.Value.GetType() == input.GetType());
        }

        [Property]
        public Property Value_should_be_int(int input) {
            ConstantValueExpression expression = new(input);
            return expression.Value.Value.Equals(input).ToProperty().And(expression.Value.Value.GetType() == input.GetType());
        }

        [Property]
        public Property Value_should_be_long(long input) {
            ConstantValueExpression expression = new(input);
            return expression.Value.Value.Equals(input).ToProperty().And(expression.Value.Value.GetType() == input.GetType());
        }

        [Property]
        public Property Value_should_be_byte(byte input) {
            ConstantValueExpression expression = new(input);
            return expression.Value.Value.Equals(input).ToProperty().And(expression.Value.Value.GetType() == input.GetType());
        }

        [Property]
        public Property Given_two_ConstantExpressions_Equals_should_depends_on_int_input_only(int input)
        {
            // Arrange
            ConstantValueExpression first = new(input);
            ConstantValueExpression second = new(input);

            // Act
            return (first.Equals(second) == Equals(first.Value, second.Value)).ToProperty();
        }

        [Property]
        public Property Given_two_ConstantExpressions_Equals_should_depends_on_long_input_only(long input)
        {
            // Arrange
            ConstantValueExpression first = new(input);
            ConstantValueExpression second = new(input);

            // Act
            return (first.Equals(second) == Equals(first.Value, second.Value)).ToProperty();
        }

        [Property]
        public Property Given_two_ConstantExpressions_Equals_should_depends_on_DateTime_input_only(DateTime input)
        {
            // Arrange
            ConstantValueExpression first = new(input);
            ConstantValueExpression second = new(input);

            // Act
            return (first.Equals(second) == Equals(first.Value, second.Value)).ToProperty();
        }

        [Property]
        public Property Given_two_ConstantExpressions_Equals_should_depends_on_DateTimeOffset_input_only(DateTimeOffset input)
        {
            // Arrange
            ConstantValueExpression first = new(input);
            ConstantValueExpression second = new(input);

            // Act
            return (first.Equals(second) == Equals(first.Value, second.Value)).ToProperty();
        }
        [Property]
        public Property Given_two_ConstantExpressions_Equals_should_depends_on_bool_input_only(bool input)
        {
            // Arrange
            ConstantValueExpression first = new(input);
            ConstantValueExpression second = new(input);

            // Act
            return (first.Equals(second) == Equals(first.Value, second.Value)).ToProperty();
        }

        [Property]
        public Property Given_two_ConstantExpressions_Equals_should_depends_on_byte_input_only(byte input)
        {
            // Arrange
            ConstantValueExpression first = new(input);
            ConstantValueExpression second = new(input);

            // Act
            return (first.Equals(second) == Equals(first.Value, second.Value)).ToProperty();
        }

        [Property]
        public Property Given_two_ConstantExpressions_Equals_should_depends_on_string_input_only(NonWhiteSpaceString input)
        {
            // Arrange
            ConstantValueExpression first = new(input.Get);
            ConstantValueExpression second = new(input.Get);

            // Act
            return (first.Equals(second) == Equals(first.Value, second.Value)).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_ConstantExpression_GetComplexity_should_return_1(ConstantValueExpression constant) => (constant.Complexity == 1).ToProperty();
    }
}

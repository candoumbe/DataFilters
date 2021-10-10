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
    public class StringValueExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public StringValueExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(StringValueExpression).Should()
                                                                           .BeAssignableTo<FilterExpression>().And
                                                                           .Implement<IEquatable<StringValueExpression>>().And
                                                                           .HaveConstructor(new[] { typeof(string) }).And
                                                                           .HaveProperty<string>("Value");

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => new StringValueExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(StringValueExpression)}'s constructor cannot be null");
        }

        [Fact]
        public void Ctor_Throws_ArgumentOutOfRangeException_When_Argument_Is_Empty()
        {
            // Act
            Action action = () => new StringValueExpression(string.Empty);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>($"The parameter of  {nameof(StringValueExpression)}'s constructor cannot be empty");
        }

        [Fact]
        public void Given_parameter_is_whitespace_Ctor_should_not_throw()
        {
            // Act
            Action action = () => new StringValueExpression(" ");

            // Assert
            action.Should()
                .NotThrow($"The parameter of {nameof(StringValueExpression)}'s constructor can be whitespace");
        }

        [Property]
        public Property Two_instances_are_equals_when_holding_same_values(NonEmptyString value)
        {
            StringValueExpression first = new(value.Item);
            StringValueExpression other = new(value.Item);

            return first.Equals(other).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_is_commutative(StringValueExpression first, StringValueExpression second) => (first.Equals(second) == second.Equals(first)).ToProperty();

        
        [Property]
        public Property Given_two_ConstantExpressions_Equals_should_depends_on_string_input_only(NonWhiteSpaceString input)
        {
            // Arrange
            StringValueExpression first = new(input.Get);
            StringValueExpression second = new(input.Get);

            // Act
            return (first.Equals(second) == Equals(first.Value, second.Value)).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_ConstantExpression_GetComplexity_should_return_1(StringValueExpression constant) => (constant.Complexity == 1).ToProperty();

        [Property]
        public void Given_StringValueExpression_and_TextExpression_are_based_on_same_value_IsEquivalentTo_should_be_true(NonWhiteSpaceString input)
        {
            // Arrange
            StringValueExpression stringValue = new(input.Item);
            TextExpression textExpression = new(input.Item);

            // Act
            bool isEquivalent = stringValue.IsEquivalentTo(textExpression);

            // Assert
            isEquivalent.Should()
                        .BeTrue($"{nameof(TextExpression)} was created from the same value");
        }
    }
}

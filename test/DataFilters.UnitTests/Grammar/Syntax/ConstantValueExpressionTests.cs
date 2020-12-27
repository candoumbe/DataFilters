using DataFilters.Grammar.Syntax;
using DataFilters.UnitTests.Helpers;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using System;
using Xunit;
using Xunit.Abstractions;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class ConstantValueExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public ConstantValueExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(ConstantValueExpression).Should()
                                                                           .BeAssignableTo<FilterExpression>().And
                                                                           .Implement<IEquatable<ConstantValueExpression>>().And
                                                                           .HaveConstructor(new[] { typeof(object) }).And
                                                                           .HaveProperty<object>("Value");

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
        public void Ctor_Throws_ArgumentOutyException_When_Argument_Is_Whitespace()
        {
            // Act
            Action action = () => new ConstantValueExpression(" ");

            // Assert
            action.Should()
                .NotThrow($"The parameter of  {nameof(ConstantValueExpression)}'s constructor can be whitespace");
        }

        [Property]
        public Property Two_instances_are_equals_when_holding_same_values(NonEmptyString value)
        {
            ConstantValueExpression first = new(value.Item);
            ConstantValueExpression other = new(value.Item);

            return first.Equals(other).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Instance_should_be_equals_to_itself(ConstantValueExpression instance) => object.Equals(instance, instance).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_is_commutative(ConstantValueExpression first, ConstantValueExpression second) => (first.Equals(second) == second.Equals(first)).ToProperty();

        [Property]
        public Property Value_should_be_DateTimeOffset(DateTimeOffset input) => Value_retains_the_underlying_type(input);

        [Property]
        public Property Value_should_be_Guid(Guid input) => Value_retains_the_underlying_type(input);

        [Property]
        public Property Value_should_be_Bool(bool input) => Value_retains_the_underlying_type(input);

        [Property]
        public Property Value_should_be_int(int input) => Value_retains_the_underlying_type(input);

        [Property]
        public Property Value_should_be_long(long input) => Value_retains_the_underlying_type(input);

        [Property]
        public Property Value_should_be_byte(byte input) => Value_retains_the_underlying_type(input);

        private Property Value_retains_the_underlying_type(object input)
        {
            ConstantValueExpression expression = new(input);

            return expression.Value.Equals(input).ToProperty()
                    .And(expression.Value.GetType() == input.GetType());
        }
    }
}

namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
using FsCheck.Fluent;
    using FsCheck.Xunit;

    using System;
    using System.Collections.Generic;

    using Xunit;
    using Xunit.Abstractions;

    public class StartsWithExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public StartsWithExpressionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void IsFilterExpression() => typeof(StartsWithExpression).Should()
                                                                        .NotBeAbstract().And
                                                                        .BeAssignableTo<FilterExpression>().And
                                                                        .Implement<IEquatable<StartsWithExpression>>().And
                                                                        .Implement<IParseableString>().And
                                                                        .NotImplement<IBoundaryExpression>();

        [Fact]
        public void Given_string_argument_is_null_Constructor_should_throw_ArgumentNullException()
        {
            // Act
            Action action = () => new StartsWithExpression((string)null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>("The parameter of the constructor cannot be null");
        }

        [Fact]
        public void Given_TextExpression_argument_is_null_Constructor_should_throw_ArgumentNullException()
        {
            // Act
            Action action = () => new StartsWithExpression((TextExpression)null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>("The parameter of the constructor cannot be null");
        }

        [Fact]
        public void Ctor_Throws_ArgumentOutOfRangeException_When_Argument_Is_Empty()
        {
            // Act
            Action action = () => new StartsWithExpression(string.Empty);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>("The parameter of the constructor cannot be empty");
        }

        [Fact]
        public void Ctor_DoesNot_Throws_ArgumentOutOfRangeException_When_Argument_Is_WhitespaceOnly()
        {
            // Act
            Action action = () => new StartsWithExpression("  ");

            // Assert
            action.Should()
                .NotThrow<ArgumentOutOfRangeException>("The parameter of the constructor can be whitespace only");
            action.Should()
                .NotThrow("The parameter of the constructor can be whitespace only");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new StartsWithExpression("prop1"),
                    new StartsWithExpression("prop1"),
                    true,
                    "comparing two different instances with same property name"
                };

                yield return new object[]
                {
                    new StartsWithExpression("prop1"),
                    new StartsWithExpression("prop2"),
                    false,
                    "comparing two different instances with different property name"
                };
            }
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_commutative(NonNull<StartsWithExpression> first, FilterExpression second)
            => (first.Item.Equals(second) == second.Equals(first.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_reflexive(NonNull<StartsWithExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_symetric(NonNull<StartsWithExpression> expression, NonNull<FilterExpression> otherExpression)
            => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty();


        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_TextExpression_as_input_EscapedParseableString_should_be_correct(NonNull<TextExpression> text)
        {
            // Arrange
            StartsWithExpression expression = new(text.Item);
            string expected = $"{text.Item.EscapedParseableString}*";

            // Act
            string actual = expression.EscapedParseableString;

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Property]
        public void Given_non_whitespace_string_as_input_as_input_EscapedParseableString_should_be_correct(NonWhiteSpaceString text)
        {
            // Arrange
            StartsWithExpression expression = new(text.Item);
            StringValueExpression stringValueExpression = new(text.Item);
            string expected = $"{stringValueExpression.EscapedParseableString}*";

            // Act
            string actual = expression.EscapedParseableString;

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_StartsWithExpression_When_right_operand_is_EndsWithExpression_Plus_operator_should_return_AndExpression(NonNull<StartsWithExpression> startsWithGen, NonNull<EndsWithExpression> endsWithGen)
        {
            // Arrange
            StartsWithExpression startsWith = startsWithGen.Item;
            EndsWithExpression endsWith = endsWithGen.Item;

            AndExpression expected = new (startsWith, endsWith);

            // Act
            AndExpression actual = startsWith + endsWith;

            // Assert
            actual.Should().Be(expected);
        }
    }
}

namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using FluentAssertions;
    using FsCheck.Xunit;
    using FsCheck;

    using System;
    using System.Collections.Generic;
    using Xunit;
    using Xunit.Abstractions;
    using DataFilters.UnitTests.Helpers;
    using FsCheck.Fluent;
    using Xunit.Categories;

    [UnitTest]
    public class EndsWithExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public EndsWithExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(EndsWithExpression).Should()
                                                                      .BeAssignableTo<FilterExpression>().And
                                                                      .Implement<IEquatable<EndsWithExpression>>().And
                                                                      .Implement<IParseableString>();

        [Fact]
        public void Given_string_input_is_null_Constructor_should_throws_ArgumentNullException()
        {
            // Act
            Action action = () => new EndsWithExpression((string)null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The value of {nameof(EndsWithExpression)}'s constructor cannot be null");
        }

        [Fact]
        public void Given_TextExpression_input_is_null_Constructor_should_throws_ArgumentNullException()
        {
            // Act
            Action action = () => new EndsWithExpression((TextExpression)null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The value of {nameof(EndsWithExpression)}'s constructor cannot be null");
        }

        [Fact]
        public void Given_string_input_is_an_empty_Constructor_should_throws_ArgumentOutOfRangeException()
        {
            // Act
            Action action = () => new EndsWithExpression(string.Empty);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>($"The value of {nameof(EndsWithExpression)}'s constructor cannot be empty");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new EndsWithExpression("prop1"),
                    new EndsWithExpression("prop1"),
                    true,
                    "comparing two different instances with same property name"
                };

                yield return new object[]
                {
                    new EndsWithExpression("prop1"),
                    new EndsWithExpression("prop2"),
                    false,
                    "comparing two different instances with different property name"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(EndsWithExpression first, object other, bool expected, string reason)
        {
            _outputHelper.WriteLine($"First instance : {first}");
            _outputHelper.WriteLine($"Second instance : {other}");

            // Act
            bool actual = first.Equals(other);
            int actualHashCode = first.GetHashCode();

            // Assert
            actual.Should()
                .Be(expected, reason);
            if (expected)
            {
                actualHashCode.Should()
                    .Be(other?.GetHashCode(), reason);
            }
            else
            {
                actualHashCode.Should()
                    .NotBe(other?.GetHashCode(), reason);
            }
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_EndsWithExpression_Complexity_should_return_1U002E5(EndsWithExpression endsWith)
            => (endsWith.Complexity == 1.5).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_TextExpression_as_input_EscapedParseableString_should_be_correct(NonNull<TextExpression> text)
        {
            // Arrange
            EndsWithExpression expression = new (text.Item);
            string expected = $"*{text.Item.EscapedParseableString}";

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
            EndsWithExpression expression = new(text.Item);
            StringValueExpression stringValueExpression = new(text.Item);
            string expected = $"*{stringValueExpression.EscapedParseableString}";

            // Act
            string actual = expression.EscapedParseableString;

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_commutative(NonNull<EndsWithExpression> first, FilterExpression second)
            => (first.Item.Equals(second) == second.Equals(first.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_reflexive(NonNull<EndsWithExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_symetric(NonNull<EndsWithExpression> expression, NonNull<FilterExpression> otherExpression)
            => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_EndsWithExpression_When_adding_AsteriskExpression_should_returns_ContainsExpression(NonNull<EndsWithExpression> endsWith)
        {
            // Arrange
            ContainsExpression expected = new(endsWith.Item.Value);

            // Act
            ContainsExpression actual = endsWith.Item + AsteriskExpression.Instance;

            // Assert
            actual.Should()
                  .Be(expected);
        }
    }
}

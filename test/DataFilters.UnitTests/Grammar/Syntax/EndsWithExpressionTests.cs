using System.Linq;
using DataFilters.Grammar.Parsing;
using DataFilters.ValueObjects;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    using System;
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;
    using FluentAssertions;
    using FsCheck;
    using FsCheck.Xunit;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    public class EndsWithExpressionTests(ITestOutputHelper outputHelper)
    {
        [Fact]
        public void IsFilterExpression() => typeof(EndsWithExpression).Should()
                                                                      .BeAssignableTo<FilterExpression>().And
                                                                      .Implement<IEquatable<EndsWithExpression>>().And
                                                                      .Implement<IProvideParseableString>();

        [Fact]
        public void Given_string_input_is_null_Constructor_should_throws_ArgumentNullException()
        {
            // Act
            Action action = () => new EndsWithExpression((EscapedString)null);

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
        public void Given_string_input_is_empty_Constructor_should_not_throw()
        {
            // Act
            Action action = () => _ = new EndsWithExpression(EscapedString.From(string.Empty));

            // Assert
            action.Should()
                .NotThrow($"The value of {nameof(EndsWithExpression)}'s constructor can be empty");
        }

        public static TheoryData<EndsWithExpression, object, bool, string> EqualsCases
            => new()
            {
                {
                    new EndsWithExpression(EscapedString.From("prop1")),
                    new EndsWithExpression(EscapedString.From("prop1")),
                    true,
                    "comparing two different instances with same property name"
                },
                {
                    new EndsWithExpression(EscapedString.From("prop1")),
                    new EndsWithExpression(EscapedString.From("prop2")),
                    false,
                    "comparing two different instances with different property name"
                }
            };

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(EndsWithExpression current, object other, bool expected, string reason)
        {
            outputHelper.WriteLine($"First instance : {current}");
            outputHelper.WriteLine($"Second instance : {other}");

            // Act
            bool actual = current.Equals(other);
            int actualHashCode = current.GetHashCode();

            // Assert
            actual.Should().Be(expected, reason);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_EndsWithExpression_Complexity_should_return_1U002E5(EndsWithExpression endsWith)
            => endsWith.Complexity.Should().Be(1.5);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_TextExpression_as_input_EscapedParseableString_should_be_correct(NonNull<TextExpression> textExpressionGenerator)
        {
            // Arrange
            TextExpression textExpression = textExpressionGenerator.Item;
            EndsWithExpression expression = new(textExpression);

            EscapedString expected = EscapedString.From($"*{textExpression.EscapedParseableString}");

            // Act
            EscapedString actual = expression.EscapedParseableString;

            // Assert
            actual.Should().Be(expected);
        }

        [Property]
        public void Given_non_whitespace_string_as_input_as_input_EscapedParseableString_should_be_correct(NonWhiteSpaceString text)
        {
            // Arrange
            string inputAsString = text.Get;
            IString input = inputAsString.Any( FilterTokenizer.SpecialCharacters.Contains)
                ? RawString.From(inputAsString)
                : EscapedString.From(inputAsString);

            EndsWithExpression expression = input switch
            {
                RawString value => new(value),
                EscapedString value => new(value),
                _ => throw new ArgumentOutOfRangeException()
            };

            StringValueExpression stringValueExpression = new(text.Item);
            string expected = $"*{stringValueExpression.EscapedParseableString}";

            // Act
            EscapedString actual = expression.EscapedParseableString;

            // Assert
            actual.Value.Should()
                  .Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_commutative(NonNull<EndsWithExpression> first, FilterExpression second)
            => first.Item.Equals(second).Should().Be(second.Equals(first.Item));

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_reflexive(NonNull<EndsWithExpression> expression)
            => expression.Item.Equals(expression.Item).Should().BeTrue();

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_symetric(NonNull<EndsWithExpression> expression, NonNull<FilterExpression> otherExpression)
            => expression.Item.Equals(otherExpression.Item).Should().Be(otherExpression.Item.Equals(expression.Item));

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
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

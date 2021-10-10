namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
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

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(StartsWithExpression first, object other, bool expected, string reason)
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
    }
}

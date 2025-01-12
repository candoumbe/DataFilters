using System.Linq;
using System.Text;
using DataFilters.Grammar.Parsing;

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

    public class ContainsExpressionTests(ITestOutputHelper outputHelper)
    {
        [Fact]
        public void IsFilterExpression() => typeof(ContainsExpression).Should()
                                                                      .BeAssignableTo<FilterExpression>().And
                                                                      .Implement<IEquatable<ContainsExpression>>().And
                                                                      .Implement<IHaveComplexity>().And
                                                                      .Implement<IParseableString>();

        [Fact]
        public void Given_string_argument_is_null_Constructor_should_thros_ArgumentNullException()
        {
            // Act
            Action action = () => _ = new ContainsExpression((string)null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void Given_TextExpression_is_null_Constructor_should_thros_ArgumentNullException()
        {
            // Act
            Action action = () => _ = new ContainsExpression((TextExpression)null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void Given_TextExpression_argument_is_null_Constructor_should_thros_ArgumentNullException()
        {
            // Act
            Action action = () => _ = new ContainsExpression(string.Empty);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>("The parameter of the constructor cannot be empty");
        }

        [Fact]
        public void Ctor_DoesNot_Throws_ArgumentOutOfRangeException_When_Argument_Is_WhitespaceOnly()
        {
            // Act
            Action action = () => _ = new ContainsExpression("  ");

            // Assert
            action.Should()
                .NotThrow<ArgumentOutOfRangeException>("The parameter of the constructor can be whitespace only");
            action.Should()
                .NotThrow("The parameter of the constructor can be whitespace only");
        }

        public static TheoryData<ContainsExpression, object, bool, string> EqualsCases
            => new()
            {
                {
                    new ContainsExpression("prop1"),
                    new ContainsExpression("prop1"),
                    true,
                    "comparing two different instances with same property name"
                },
                {
                    new ContainsExpression("prop1"),
                    new ContainsExpression("prop2"),
                    false,
                    "comparing two different instances with different property name"
                }
            };

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(ContainsExpression first, object other, bool expected, string reason)
        {
            outputHelper.WriteLine($"First instance : {first}");
            outputHelper.WriteLine($"Second instance : {other}");

            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_ContainsExpression_Complexity_eq_1U002E5(ContainsExpression contains)
        {
            // Act
            double actual = contains.Complexity;

            // Assert
            actual.Should().Be(1.5);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void IsEquivalentTo_should_be_reflexive(ContainsExpression contains)
            => contains.IsEquivalentTo(contains).Should().BeTrue();

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_TextExpression_as_input_EscapedParseableString_should_be_correct(NonNull<TextExpression> textExpressionGenerator)
        {
            // Arrange
            TextExpression textExpression = textExpressionGenerator.Item;
            ContainsExpression expression = new(textExpression);

            string expected = $"*{textExpression.EscapedParseableString}*";

            // Act
            string actual = expression.EscapedParseableString;

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Property]
        public void Given_non_whitespace_string_as_input_Then_EscapedParseableString_should_be_correct(NonWhiteSpaceString textGenerator)
        {
            // Arrange
            string text = textGenerator.Item;
            ContainsExpression expression = new(text);

            StringBuilder sb = new (text.Length * 2);
            foreach (char chr in text)
            {
                if (FilterTokenizer.SpecialCharacters.Contains(chr))
                {
                    sb = sb.Append('\\');
                }
                sb = sb.Append(chr);
            }
            string expected = $"*{sb}*";

            // Act
            string actual = expression.EscapedParseableString;

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_commutative(NonNull<ContainsExpression> firstGenerator, FilterExpression second)
        {
            // Arrange
            ContainsExpression first = firstGenerator.Item;

            // Act
            bool firstEqualsSecond = first.Equals(second);
            bool secondEqualsFirst = second.Equals(first);

            // Assert
            firstEqualsSecond.Should().Be(secondEqualsFirst, "'equals' implementation must be commutative");
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_reflexive(NonNull<ContainsExpression> expression)
        {
            // Act
            bool actual = expression.Item.Equals(expression.Item);

            // Assert
            actual.Should().BeTrue("'equals' implementation must be reflexive");
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_symmetric(NonNull<ContainsExpression> expression, NonNull<FilterExpression> otherExpression)
        {
            // Act

            bool expressionEqualsOther = expression.Item.Equals(otherExpression.Item);
            bool otherEqualsExpression = otherExpression.Item.Equals(expression.Item);

            // Assert
            expressionEqualsOther.Should().Be(otherEqualsExpression, "'equals' implementation must be symmetric");
        }

        /// <summary>
        /// Tests the constructor by copy of <see cref="TextExpression"/>.
        /// </summary>
        /// <param name="textExpressionGenerator"></param>
        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_constructor_argument_is_TextExpression_Then_ContainsExpression_should_be_correct(NonNull<TextExpression> textExpressionGenerator)
        {
            // Arrange
            TextExpression textExpression = textExpressionGenerator.Item;
            outputHelper.WriteLine($"TextExpression : {textExpression.EscapedParseableString}");

            // Act
            ContainsExpression containsExpression = new (textExpression);

            // Assert
            containsExpression.EscapedParseableString.Should()
                .Be($"*{textExpression.EscapedParseableString}*");
        }

        public static TheoryData<string, string> EscapedParseableStringCases
            => new()
            {
                { "Oi\fj8G]t:JK%H6m>+r{)[5\n6", "*Oi\fj8G\\]t\\:JK%H6m>+r\\{\\)\\[5\n6*" }
            };

        [Theory]
        [MemberData(nameof(EscapedParseableStringCases))]
        public void Given_input_as_string_Then_the_constructor_should_properly_escape_it(string input, string expected)
        {
            // Arrange
            ContainsExpression expression = new(input);

            // Act
            string actual = expression.EscapedParseableString;

            // Assert
            actual.Should().Be(expected);
        }
    }
}

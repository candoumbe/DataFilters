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

    public class ContainsExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public ContainsExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

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
            Action action = () => new ContainsExpression((string)null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void Given_TextExpression_is_null_Constructor_should_thros_ArgumentNullException()
        {
            // Act
            Action action = () => new ContainsExpression((TextExpression)null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void Given_TextExpression_argument_is_null_Constructor_should_thros_ArgumentNullException()
        {
            // Act
            Action action = () => new ContainsExpression(string.Empty);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>("The parameter of the constructor cannot be empty");
        }

        [Fact]
        public void Ctor_DoesNot_Throws_ArgumentOutOfRangeException_When_Argument_Is_WhitespaceOnly()
        {
            // Act
            Action action = () => new ContainsExpression("  ");

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
                    new ContainsExpression("prop1"),
                    new ContainsExpression("prop1"),
                    true,
                    "comparing two different instances with same property name"
                };

                yield return new object[]
                {
                    new ContainsExpression("prop1"),
                    new ContainsExpression("prop2"),
                    false,
                    "comparing two different instances with different property name"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(ContainsExpression first, object other, bool expected, string reason)
        {
            _outputHelper.WriteLine($"First instance : {first}");
            _outputHelper.WriteLine($"Second instance : {other}");

            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_ContainsExpression_Complexity_eq_1U002E5(ContainsExpression contains)
            => (contains.Complexity == 1.5).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_ConstainsExpression_calling_IsEquivalentTo_with_itself_should_return_true(ContainsExpression contains)
            => (contains.IsEquivalentTo(contains)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_TextExpression_as_input_EscapedParseableString_should_be_correct(NonNull<TextExpression> text)
        {
            // Arrange
            ContainsExpression expression = new(text.Item);
            string expected = $"*{text.Item.EscapedParseableString}*";

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
            ContainsExpression expression = new(text.Item);
            StringValueExpression stringValueExpression = new(text.Item);
            string expected = $"*{stringValueExpression.EscapedParseableString}*";

            // Act
            string actual = expression.EscapedParseableString;

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_commutative(NonNull<ContainsExpression> first, FilterExpression second)
            => (first.Item.Equals(second) == second.Equals(first.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_reflexive(NonNull<ContainsExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_symetric(NonNull<ContainsExpression> expression, NonNull<FilterExpression> otherExpression)
            => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty();

    }
}

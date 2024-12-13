using System.Linq;
using System.Text;
using Candoumbe.MiscUtilities.Comparers;
using DataFilters.Grammar.Parsing;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    using System;
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;
    using FluentAssertions;
    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    public class StringValueExpressionTests(ITestOutputHelper outputHelper)
    {
        [Fact]
        public void IsFilterExpression() => typeof(StringValueExpression).Should()
                                                                           .BeAssignableTo<FilterExpression>().And
                                                                           .Implement<IEquatable<StringValueExpression>>();

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => _ = new StringValueExpression((string)null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(StringValueExpression)}'s constructor cannot be null");
        }

        [Fact]
        public void Ctor_Throws_ArgumentOutOfRangeException_When_Argument_Is_Empty()
        {
            // Act
            Action action = () => _ = new StringValueExpression(string.Empty);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>($"The parameter of  {nameof(StringValueExpression)}'s constructor cannot be empty");
        }

        [Fact]
        public void Given_parameter_is_whitespace_Ctor_should_not_throw()
        {
            // Act
            Action action = () => _ = new StringValueExpression(" ");

            // Assert
            action.Should()
                .NotThrow($"The parameter of {nameof(StringValueExpression)}'s constructor can be whitespace");
        }

        [Property]
        public void Given_two_instances_that_hold_values_that_are_equals_Equals_should_return_true(NonEmptyString value)
        {
            // Arrange
            StringValueExpression first = new(value.Item);
            StringValueExpression other = new(value.Item);

            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                  .BeTrue();
        }

        public static TheoryData<StringValueExpression, object, bool> EqualsCases
            => new()
            {
                {
                    new StringValueExpression("True"),
                    new OrExpression(new StringValueExpression("True"), new StringValueExpression("True")),
                    false
                },
                {
                    new StringValueExpression("0"),
                    new NumericValueExpression("0"),
                    true
                },
                {
                    new StringValueExpression("True"),
                    new StringValueExpression("True"),
                    true
                }
            };

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void Equals_should_behave_as_expected(StringValueExpression stringValue, object obj, bool expected)
        {
            // Act
            bool actual = stringValue.Equals(obj);

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_commutative(StringValueExpression first, FilterExpression second)
            => (first.Equals(second) == second.Equals(first)).ToProperty().QuickCheckThrowOnFailure();

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_reflexive(NonNull<StringValueExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty();

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_symetric(NonNull<StringValueExpression> expression, NonNull<FilterExpression> otherExpression)
            => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty();

        [Property]
        public void Given_two_StringValueExpresssions_Equals_should_depends_on_string_input_only(NonWhiteSpaceString input)
        {
            // Arrange
            StringValueExpression first = new(input.Get);
            StringValueExpression second = new(input.Get);

            // Act
            (first.Equals(second) == first.Value.IsEquivalentTo(second.Value, CharComparer.Ordinal))
                .ToProperty()
                .QuickCheckThrowOnFailure(outputHelper);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_StringValueExpression_GetComplexity_should_return_1(StringValueExpression constant) => (Math.Abs(constant.Complexity - 1) < float.Epsilon).ToProperty();

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

        [Property]
        public void Given_StringValueExpression_when_GroupExpression_holds_an_expression_that_is_equals_to_that_StringValueExpression_IsEquivalentTo_should_return_true(NonWhiteSpaceString input)
        {
            // Arrange
            StringValueExpression stringValue = new(input.Item);
            GroupExpression group = new(new StringValueExpression(input.Item));

            // Act
            bool actual = stringValue.IsEquivalentTo(group);

            // Assert
            actual.Should()
                  .BeTrue();
        }

        [Property]
        public void Given_input_as_string_Then_EscapedParseableString_should_be_correct(NonWhiteSpaceString inputGenerator)
        {
            // Arrange
            string input = inputGenerator.Item;
            StringValueExpression expression = new(input);
            StringBuilder sbExpected = new (input.Length * 2);

            foreach (char c in input)
            {
                if (FilterTokenizer.SpecialCharacters.Contains(c))
                {
                    sbExpected.Append('\\');
                }
                sbExpected.Append(c);
            }

            string expected = sbExpected.ToString();

            // Act
            string actual = expression.EscapedParseableString;

            // Assert
            actual.Should().Be(expected);
        }
    }
}

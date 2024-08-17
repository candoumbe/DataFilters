using DataFilters.Grammar.Syntax;
using DataFilters.UnitTests.Helpers;

using FluentAssertions;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    [UnitTest]
    public class TextExpressionTests(ITestOutputHelper outputHelper)
    {
        [Theory]
        [InlineData(@"<\\Y!A", @"""<\\\\Y!A""")]
        public void Given_input_EscapedParseableString_should_be_correct(string input, string expected)
        {
            // Arrange
            TextExpression text = new(input);

            // Act
            string actual = text.EscapedParseableString;

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Property]
        public void Equals_to_null_always_returns_false(NonEmptyString input)
        {
            // Arrange
            TextExpression textExpression = new(input.Item);

            // Act
            bool actual = textExpression.Equals(null);

            // Assert
            actual.Should()
                  .BeFalse();
        }

        [Property]
        public void Given_two_instances_with_same_input_Equals_should_return_true(NonWhiteSpaceString input)
        {
            // Arrange
            outputHelper.WriteLine($"input : '{input.Item}'");
            TextExpression first = new(input.Item);
            TextExpression other = new(input.Item);

            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                  .BeTrue();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_commutative(NonNull<TextExpression> first, FilterExpression second)
            => (first.Item.Equals(second) == second.Equals(first.Item)).ToProperty()
                .QuickCheckThrowOnFailure(outputHelper);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_reflexive(NonNull<TextExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty()
                .QuickCheckThrowOnFailure(outputHelper);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_symmetric(NonNull<TextExpression> expression, NonNull<FilterExpression> otherExpression)
            => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty();

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_non_null_TextExpression_EscapedParseableString_should_be_correct(NonNull<TextExpression> text)
        {
            // Arrange
            TextExpression textExpression = text.Item;
            string expected = $@"""{textExpression.OriginalString.Replace("\\", @"\\").Replace(@"""", @"\""")}""";
            // Act
            string escapedParseableString = textExpression.EscapedParseableString;

            // Assert
            escapedParseableString.Should()
                                  .StartWith(@"""").And
                                  .EndWith(@"""").And
                                  .Be(expected);
        }
    }
}

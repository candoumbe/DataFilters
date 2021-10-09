using DataFilters.Grammar.Syntax;
using DataFilters.UnitTests.Helpers;

using FluentAssertions;

using FsCheck;
using FsCheck.Xunit;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    [UnitTest]
    public class TextExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public TextExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

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
            _outputHelper.WriteLine($"input : '{input.Item}'");
            TextExpression first = new(input.Item);
            TextExpression other = new(input.Item);

            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                  .BeTrue();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Equals_should_be_reflexive(NonNull<TextExpression> input)
        {
            // Arrange
            TextExpression current = input.Item;

            // Act
            bool actual = current.Equals(current);

            // Assert
            actual.Should()
                  .BeTrue();
        }
    }
}

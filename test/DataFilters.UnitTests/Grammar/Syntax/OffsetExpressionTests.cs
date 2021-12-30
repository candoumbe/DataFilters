namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;

    using FluentAssertions;

    using Xunit;
    using Xunit.Categories;
    using FsCheck.Xunit;
    using DataFilters.UnitTests.Helpers;
    using FsCheck;
    using FsCheck.Fluent;

    [UnitTest]
    public class OffsetExpressionTests
    {
        [Theory]
        [InlineData(NumericSign.Minus, 0, 0, "Z")]
        [InlineData(NumericSign.Plus, 0, 0, "Z")]
        [InlineData(NumericSign.Minus, 1, 0, "-01:00")]
        [InlineData(NumericSign.Plus, 2, 0, "+02:00")]
        [InlineData(NumericSign.Minus, 2, 0, "-02:00")]
        public void Given_input_EscapedParseableString_should_be_correct(NumericSign sign, uint hours, uint minutes, string expected)
        {
            // Arrange
            OffsetExpression offset = new(sign, hours, minutes);

            // Act
            string actual = offset.EscapedParseableString;

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_an_OffsetExpression_instance_Equals_should_be_reflective(NonNull<OffsetExpression> offset) => offset.Item.Equals(offset.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_commutative(NonNull<OffsetExpression> first, FilterExpression second)
            => (first.Item.Equals(second) == second.Equals(first.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_reflexive(NonNull<OffsetExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_symetric(NonNull<OffsetExpression> expression, NonNull<FilterExpression> otherExpression)
            => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty();

    }
}

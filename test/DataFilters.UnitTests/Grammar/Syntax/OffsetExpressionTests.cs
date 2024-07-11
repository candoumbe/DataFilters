namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;

    using Xunit;
    using Xunit.Categories;

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

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_an_OffsetExpression_instance_Equals_should_be_reflective(NonNull<OffsetExpression> offset) => offset.Item.Equals(offset.Item).ToProperty();

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_commutative(NonNull<OffsetExpression> first, FilterExpression second)
            => first.Item.Equals(second).Should().Be(second.Equals(first.Item));

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_reflexive(NonNull<OffsetExpression> expression)
            => expression.Item.Should().Be(expression.Item);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_symetric(NonNull<OffsetExpression> expression, NonNull<FilterExpression> otherExpression)
            => expression.Item.Equals(otherExpression.Item).Should().Be(otherExpression.Item.Equals(expression.Item));

        [Property]
        public void Given_offset_is_Zero_When_comparing_same_offset_should_not_take_into_account_the_numeric_sign(NumericSign sign)
        {
            // Arrange
            OffsetExpression zero = OffsetExpression.Zero;
            OffsetExpression zeroWithNegativeSign = new(sign, (uint)zero.Hours, (uint)zero.Minutes);

            // Act
            bool actual = zero == zeroWithNegativeSign;

            // Assert
            actual.Should().BeTrue();
        }
    }
}

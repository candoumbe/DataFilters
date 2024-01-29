namespace DataFilters.UnitTests.Grammar.Syntax
{
    using System;
    using System.Linq;
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;
    using FluentAssertions;
    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;
    using Xunit;
    using Xunit.Abstractions;

    public class RangeBracketValueTests(ITestOutputHelper outputHelper)
    {
        [Property]
        public void Value_should_be_set_by_the_parameter_of_the_constructor(char start, char end)
        {
            // Act
            RangeBracketValue rangeBracketValue = new(start, end);

            // Assert
            rangeBracketValue.Start.Should().Be(start);
            rangeBracketValue.End.Should().Be(end);
        }

        [Property]
        public void Given_ConstantBracketExpression_contains_consecutive_characters_When_comparing_to_RangeBracketValue_that_has_head_and_tail_Equals_should_returns_true()
        {
            // Arrange
            IArbMap arbMap = ArbMap.Default;
            Prop.ForAll(arbMap.ArbFor<char>().Filter(char.IsLetter).Filter(char.IsLower), arbMap.ArbFor<char>().Filter(char.IsLetter).Filter(char.IsLower),
                               (start, end) =>
                                {
                                    char[] chrs = [.. new[] { start, end }.OrderBy(x => x)
];
                                    char head = chrs[0];
                                    char tail = chrs[1];
                                    ConstantBracketValue constantBracketValue = new(Enumerable.Range(head, tail - head + 1)
                                                                                              .Select(ascii => ((char)ascii).ToString())
                                                                                              .Aggregate((accumulate, current) => $"{accumulate}{current}"));

                                    // Act
                                    bool actual = new RangeBracketValue(head, tail).Equals(constantBracketValue);

                                    return actual.When(start < end);
                                })
                .QuickCheckThrowOnFailure(outputHelper);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_left_and_right_RangeBracketValues_left_ne_right_should_be_same_as_not_left_Equals_right(RangeBracketValue left, RangeBracketValue right)
        {
            // Arrange
            bool expected = !left.Equals(right);

            // Act
            bool actual = left != right;

            // Assert
            actual.Should().Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_left_and_right_RangeBracketValues_left_gt_right_should_be_same_as_left_Start_gt_right_Start(RangeBracketValue left, RangeBracketValue right)
        {
            // Arrange
            bool expected = left.Start > right.Start;

            // Act
            bool actual = left > right;

            // Assert
            actual.Should().Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_left_and_right_RangeBracketValues_left_lte_right_should_be_same_as_left_lt_right_or_left_eq_right(RangeBracketValue left, RangeBracketValue right)
        {
            // Arrange
            bool expected = left < right || left == right;

            // Act
            bool actual = left <= right;

            // Assert
            actual.Should().Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_left_and_right_RangeBracketValues_left_gte_right_should_be_same_as_left_gt_right_or_left_eq_right(RangeBracketValue left, RangeBracketValue right)
        {
            // Arrange
            bool expected = left > right || left == right;

            // Act
            bool actual = left >= right;

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData('a', 'a', 2)]
        public void Complexity_should_behave_as_expected(char start, char end, double expected)
        {
            // Arrange
            RangeBracketValue bracketValue = new(start, end);

            // Act
            double actual = bracketValue.Complexity;

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_RangeBracketValue_Complexity_should_depends_on_start_and_end(RangeBracketValue bracketValue)
        {
            // Arrange
            double expected = 1 + Math.Pow(2, bracketValue.End - bracketValue.Start);

            // Act
            double actual = bracketValue.Complexity;

            // Assert
            actual.Should()
                  .Be(expected);
        }
    }
}

namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FsCheck;
    using FsCheck.Xunit;

    using System.Linq;

    public class RangeBracketValueTests
    {
        [Property]
        public Property Value_should_be_set_by_the_parameter_of_the_constructor(char start, char end)
        {
            // Act
            RangeBracketValue rangeBracketValue = new(start, end);

            // Assert
            return ((start, end).Equals((rangeBracketValue.Start, rangeBracketValue.End))).ToProperty();
        }

        [Property]
        public Property Given_ConstantBracketExpression_contains_consecutive_characters_Equals_should_returns_true()
        {
            // Arrange
            return Prop.ForAll(Arb.Default.Char().Filter(char.IsLetter).Filter(char.IsLower) , Arb.Default.Char().Filter(char.IsLetter).Filter(char.IsLower),
                               (start, end) =>
                                {
                                    char[] chrs = new[] { start, end }.OrderBy(x => x)
                                                                      .ToArray();
                                    char head = chrs[0];
                                    char tail = chrs[1];
                                    ConstantBracketValue constantBracketValue = new(Enumerable.Range(head, tail - head + 1)
                                                                                              .Select(ascii => ((char)ascii).ToString())
                                                                                              .Aggregate((accumulate, current) => $"{accumulate}{current}"));

                                    // Act
                                    bool actual = new RangeBracketValue(head, tail).Equals(constantBracketValue);

                                    return actual.When(start < end);
                                });
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_left_and_right_RangeBracketValues_left_ne_right_should_be_same_as_not_left_Equals_right(RangeBracketValue left, RangeBracketValue right)
            => (left != right).When(!left.Equals(right)).Label($"Left, Right : {(left, right)}");

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_left_and_right_RangeBracketValues_left_gt_right_should_be_same_as_left_Start_gt_right_Start(RangeBracketValue left, RangeBracketValue right)
            => (left > right).When(left.Start > right.Start).Label($"Left, Right : {(left, right)}");

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_left_and_right_RangeBracketValues_left_lte_right_should_be_same_as_left_lt_right_or_left_eq_right(RangeBracketValue left, RangeBracketValue right)
            => (left <= right).When(left < right || left == right).Label($"Left, Right : {(left, right)}");

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_left_and_right_RangeBracketValues_left_gte_right_should_be_same_as_left_gt_right_or_left_eq_right(RangeBracketValue left, RangeBracketValue right)
            => (left <= right).When(left < right || left == right).Label($"Left, Right : {(left, right)}");
    }
}

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
    using Xunit.Categories;

    [UnitTest]
    public class DurationExpressionTests
    {
        [Fact]
        public void IsFilterExpression() => typeof(DurationExpression).Should()
                                                                      .BeDerivedFrom<FilterExpression>();

        [Property]
        public Property Throws_ArgumentNullException_when_any_ctor_input_is_negative(int years, int months, int weeks, int days, int hours, int minutes, int seconds)
        {
            Action invokingConstructor = () => new DurationExpression(years, months, weeks, days, hours, minutes, seconds);

            return ((Action) (() => invokingConstructor.Should().ThrowExactly<ArgumentOutOfRangeException>()))
                              .When(years < 0 || months < 0 || weeks < 0 || days < 0 || hours < 0 || minutes < 0 || seconds < 0);
        }

        [Property]
        public Property Set_properties_accordingly(PositiveInt years, PositiveInt months, PositiveInt weeks, PositiveInt days, PositiveInt hours, PositiveInt minutes, PositiveInt seconds)
        {
            DurationExpression duration = new(years.Item, months.Item, weeks.Item, days.Item, hours.Item, minutes.Item, seconds.Item);

            return (duration.Years == years.Item).ToProperty()
                   .And(duration.Months == months.Item)
                   .And(duration.Weeks == weeks.Item)
                   .And(duration.Days == days.Item)
                   .And(duration.Hours == hours.Item)
                   .And(duration.Minutes == minutes.Item)
                   .And(duration.Seconds == seconds.Item);
        }

        [Property]
        public Property Equals_depends_on_values_only(PositiveInt years, PositiveInt months, PositiveInt weeks, PositiveInt days, PositiveInt hours, PositiveInt minutes, PositiveInt seconds)
        {
            DurationExpression first = new(years.Item, months.Item, weeks.Item, days.Item, hours.Item, minutes.Item, seconds.Item);
            DurationExpression other = new(years.Item, months.Item, weeks.Item, days.Item, hours.Item, minutes.Item, seconds.Item);

            return first.Equals(other).Label("Equality")
                    .And(first.GetHashCode() == other.GetHashCode()).Label("Hashcode");
        }

        [Property]
        public Property Two_durations_that_are_equal_are_equivalent(PositiveInt years, PositiveInt months, PositiveInt weeks, PositiveInt days, PositiveInt hours, PositiveInt minutes, PositiveInt seconds)
        {
            DurationExpression first = new(years.Item, months.Item, weeks.Item, days.Item, hours.Item, minutes.Item, seconds.Item);
            DurationExpression other = new(years.Item, months.Item, weeks.Item, days.Item, hours.Item, minutes.Item, seconds.Item);

            return first.Equals(other).And(first.IsEquivalentTo(other));
        }

        [Property]
        public Property Two_durations_are_equivalent_when_they_represent_same_timespan()
        {
            DurationExpression first = new(hours: 24);
            DurationExpression other = new(days: 1);

            return (!first.Equals(other)).Label("Equality")
                .And(first.IsEquivalentTo(other)).Label("Equivalency");
        }

        [Property]
        public Property Equals_to_null_always_returns_false(PositiveInt years, PositiveInt months, PositiveInt weeks, PositiveInt days, PositiveInt hours, PositiveInt minutes, PositiveInt seconds)
        {
            DurationExpression duration = new(years.Item, months.Item, weeks.Item, days.Item, hours.Item, minutes.Item, seconds.Item);

            return (!duration.Equals(null)).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_commutative(NonNull<DurationExpression> first, FilterExpression second)
            => (first.Item.Equals(second) == second.Equals(first.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_reflexive(NonNull<DurationExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_symetric(NonNull<DurationExpression> expression, NonNull<FilterExpression> otherExpression)
            => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty();


        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_DurationExpression_GetComplexity_should_return_1(DurationExpression duration) => (duration.Complexity == 1).ToProperty();
    }
}
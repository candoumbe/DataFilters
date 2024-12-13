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

namespace DataFilters.UnitTests.Grammar.Syntax
{

    [UnitTest]
    public class TimeExpressionTests(ITestOutputHelper outputHelper)
    {
        [Fact]
        public void IsFilterExpression() => typeof(TimeExpression).Should()
                                            .BeAssignableTo<FilterExpression>().And
                                            .Implement<IEquatable<TimeExpression>>().And
                                            .HaveConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int) }).And
                                            .HaveProperty<int>("Hours").And
                                            .HaveProperty<int>("Minutes").And
                                            .HaveProperty<int>("Seconds").And
                                            .HaveProperty<int>("Milliseconds");

        [Property]
        public void Ctor_should_build_valid_instance(IntWithMinMax hours,
                                                     IntWithMinMax minutes,
                                                     IntWithMinMax seconds,
                                                     IntWithMinMax milliseconds)
        {
            outputHelper.WriteLine($"hours : {hours.Item}");
            outputHelper.WriteLine($"minutes : {minutes.Item}");
            outputHelper.WriteLine($"seconds : {seconds.Item}");
            outputHelper.WriteLine($"milliseconds : {milliseconds.Item}");

            // Arrange
            Lazy<TimeExpression> timeExpressionBuilder = new(() => new TimeExpression(hours.Item, minutes.Item,
                                                                                      seconds.Item, milliseconds.Item));

            Action invokingCtor = () => { TimeExpression value = timeExpressionBuilder.Value; };

            ((Action)(() => invokingCtor.Should().ThrowExactly<ArgumentOutOfRangeException>())).When(hours.Item < 0
                                                                                                 || minutes.Item < 0 || 59 < minutes.Item
                                                                                                 || seconds.Item < 0 || 60 < seconds.Item
                                                                                                 || (seconds.Item == 60 && minutes.Item != 59 && hours.Item != 23)
                                                                                                 )
                    .Label("Invalid TimeExpression").Trivial(hours.Item < 0
                                                             || minutes.Item < 0 || 59 < minutes.Item
                                                             || seconds.Item < 0 || 60 < seconds.Item
                                                             || (seconds.Item == 60 && minutes.Item != 59 && hours.Item != 23))
                .And(() =>
                {
                    TimeExpression timeExpression = timeExpressionBuilder.Value;
                    return timeExpression.Hours == hours.Item
                           && timeExpression.Minutes == minutes.Item
                           && timeExpression.Seconds == seconds.Item;
                })
                .VerboseCheck(outputHelper);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_commutative(NonNull<TimeExpression> first, FilterExpression second)
            => (first.Item.Equals(second) == second.Equals(first.Item)).ToProperty()
                .QuickCheckThrowOnFailure(outputHelper);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_reflexive(NonNull<TimeExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty().QuickCheckThrowOnFailure(outputHelper);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_symetric(NonNull<TimeExpression> expression, NonNull<FilterExpression> otherExpression)
            => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty().QuickCheckThrowOnFailure(outputHelper);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_non_null_TimeExpression_instance_should_never_be_equal_to_null(TimeExpression instance)
        {
            // Act
            bool actual = instance.Equals(null);

            // Assert
            actual.Should()
                  .BeFalse();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_two_TimeExpression_instances_that_have_same_values_should_be_equal(NonNegativeInt hours, NonNegativeInt minutes, NonNegativeInt seconds, NonNegativeInt milliseconds)
        {
            // Arrange
            TimeExpression first = new(hours.Item, minutes.Item, seconds.Item, milliseconds.Item);
            TimeExpression other = new(hours.Item, minutes.Item, seconds.Item, milliseconds.Item);

            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                  .BeTrue();
            first.GetHashCode().Should()
                               .Be(other.GetHashCode());
        }

        [Bug(32)]
        [Theory]
        [InlineData(00, 00, 00, 00, "00:00:00")]
        [InlineData(02, 53, 39, 987, "02:53:39.987")]
        public void Given_TimeExpression_instance_EscapedParseableString_should_be_in_expected_form(int hours,
                                                                                                    int minutes,
                                                                                                    int seconds,
                                                                                                    int milliseconds,
                                                                                                    string expected)
        {
            // Arrange
            TimeExpression timeExpression = new(hours, minutes, seconds, milliseconds);

            // Act
            string actual = timeExpression.EscapedParseableString;

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void IsEquivalent_should_be_commutative(NonNull<TimeExpression> first, FilterExpression second)
        {
            // Act
            bool actual = first.Item.IsEquivalentTo(second);
            bool expected = second.IsEquivalentTo(first.Item);

            // Assert
            actual.Should().Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void IsEquivalentTo_should_be_reflexive(NonNull<TimeExpression> expression)
            => expression.Item.IsEquivalentTo(expression.Item).Should().BeTrue();

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void IsEquivalentTo_should_be_symetric(NonNull<TimeExpression> expression, NonNull<FilterExpression> otherExpression)
            => expression.Item.IsEquivalentTo(otherExpression.Item).Should().Be(otherExpression.Item.IsEquivalentTo(expression.Item));
    }
}

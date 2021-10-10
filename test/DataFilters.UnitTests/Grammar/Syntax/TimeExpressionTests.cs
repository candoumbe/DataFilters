namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;

    using System;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    public class TimeExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public TimeExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(TimeExpression).Should()
                                            .BeAssignableTo<FilterExpression>().And
                                            .Implement<IEquatable<TimeExpression>>().And
                                            .HaveConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int)}).And
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
            _outputHelper.WriteLine($"hours : {hours.Item}");
            _outputHelper.WriteLine($"minutes : {minutes.Item}");
            _outputHelper.WriteLine($"seconds : {seconds.Item}");
            _outputHelper.WriteLine($"milliseconds : {milliseconds.Item}");

            // Arrange
            Lazy<TimeExpression> timeExpressionBuilder = new(() => new TimeExpression(hours.Item, minutes.Item,
                                                                                      seconds.Item, milliseconds.Item));

            Action invokingCtor = () => { var value = timeExpressionBuilder.Value; };

            ((Action) (() => invokingCtor.Should().ThrowExactly<ArgumentOutOfRangeException>())).When(hours.Item < 0
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
                .VerboseCheck(_outputHelper);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_TimeExpression_instance_should_be_equal_to_itself(TimeExpression instance)
        {
            // Act
            bool actual = instance.Equals(instance);

            // Assert
            actual.Should()
                  .BeTrue();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_non_null_TimeExpression_instance_should_never_be_equal_to_null(TimeExpression instance)
        {
            // Act
            bool actual = instance.Equals(null);

            // Assert
            actual.Should()
                  .BeFalse();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
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
    }
}

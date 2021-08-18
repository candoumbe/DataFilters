
namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Xunit;

    using System;
    using System.Collections.Generic;

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
                                            .HaveConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(TimeOffset) }).And
                                            .HaveProperty<int>("Hours").And
                                            .HaveProperty<int>("Minutes").And
                                            .HaveProperty<int>("Seconds").And
                                            .HaveProperty<int>("Milliseconds").And
                                            .HaveProperty<TimeOffset>("Offset");

        [Property]
        public void Ctor_should_build_valid_instance(IntWithMinMax hours,
                                                     IntWithMinMax minutes,
                                                     IntWithMinMax seconds,
                                                     IntWithMinMax milliseconds,
                                                     IntWithMinMax offsetHours,
                                                     IntWithMinMax offsetMinutes)
        {
            _outputHelper.WriteLine($"hours : {hours.Item}");
            _outputHelper.WriteLine($"minutes : {minutes.Item}");
            _outputHelper.WriteLine($"seconds : {seconds.Item}");
            _outputHelper.WriteLine($"milliseconds : {milliseconds.Item}");
            _outputHelper.WriteLine($"offsetHours : {offsetHours.Item}");
            _outputHelper.WriteLine($"offsetMinutes : {offsetMinutes.Item}");

            // Arrange
            Lazy<TimeExpression> timeExpressionBuilder = new(() => new TimeExpression(hours.Item, minutes.Item,
                                                                                      seconds.Item, milliseconds.Item,
                                                                                      new TimeOffset(offsetHours.Item, offsetMinutes.Item)));

            Prop.Throws<ArgumentOutOfRangeException, TimeExpression>(timeExpressionBuilder).When(hours.Item < 0
                                                                                                 || minutes.Item < 0 || 59 < minutes.Item
                                                                                                 || seconds.Item < 0 || 60 < seconds.Item
                                                                                                 || (seconds.Item == 60 && minutes.Item != 59 && hours.Item != 23)
                                                                                                 || offsetMinutes.Item < 0 || 59 < offsetMinutes.Item
                                                                                                 )
                    .Label("Invalid TimeExpression").Trivial(hours.Item < 0
                                                             || minutes.Item < 0 || 59 < minutes.Item
                                                             || seconds.Item < 0 || 60 < seconds.Item
                                                             || (seconds.Item == 60 && minutes.Item != 59 && hours.Item != 23)
                                                             || offsetMinutes.Item < 0 || 59 < offsetMinutes.Item)
                .And(() =>
                {
                    TimeExpression timeExpression = timeExpressionBuilder.Value;
                    return timeExpression.Hours == hours.Item
                           && timeExpression.Minutes == minutes.Item
                           && timeExpression.Seconds == seconds.Item
                           && timeExpression.Offset == null;
                })
                .VerboseCheck(_outputHelper);
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new TimeExpression(hours: 10, minutes:10, seconds: 1),
                    new TimeExpression(hours: 10, minutes:10, seconds: 1),
                    true,
                    $"comparing two {nameof(TimeExpression)} instance with same value for each field"
                };
                {
                    TimeExpression instance = new(hours: 2050, minutes: 10, seconds: 14);

                    yield return new object[]
                    {
                        instance,
                        instance,
                        true,
                        "comparing an instance to itself"
                    };

                    yield return new object[]
                    {
                        instance,
                        null,
                        false,
                        "comparing an instance to null"
                    };

                    yield return new object[]
                    {
                        instance,
                        new TimeExpression(instance.Hours, instance.Minutes, instance.Seconds - 1),
                        false,
                        $"comparing two {nameof(TimeExpression)} instance with that differs only by the {nameof(TimeExpression.Seconds)} value"
                    };

                    yield return new object[]
                    {
                        instance,
                        new TimeExpression(instance.Hours, instance.Minutes - 1, instance.Seconds),
                        false,
                        $"comparing two {nameof(TimeExpression)} instance with that differs only by the {nameof(TimeExpression.Minutes)} value"
                    };

                    yield return new object[]
                    {
                        instance,
                        new TimeExpression(instance.Hours - 1, instance.Minutes, instance.Seconds),
                        false,
                        $"comparing two {nameof(TimeExpression)} instance with that differs only by the {nameof(TimeExpression.Hours)} value"
                    };
                }

                yield return new object[]
                {
                    new TimeExpression(hours: 10, minutes:10, seconds: 1),
                    new TimeExpression(hours: 10, minutes:10, seconds: 1, offset : new (hours : 2, minutes : 10)),
                    false,
                    $"comparing two {nameof(TimeExpression)} instances with same time but not same offset"
                };
            }
        }

        [Property(Arbitrary = new[] {typeof(ExpressionsGenerators)})]
        public void An_instance_of_TimeExpression_should_be_equal_to_itself(TimeExpression time)
        {
            // Act
            bool actual = time.Equals(time);

            // Assert
            actual.Should()
                  .BeTrue();

        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void An_instance_of_TimeExpression_should_not_be_equal_to_null(TimeExpression time)
        {
            // Act
            bool actual = time.Equals(null);

            // Assert
            actual.Should()
                  .BeFalse();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Two_instances_built_from_same_data_should_be_equal(PositiveInt hours,
                                                                       PositiveInt minutes,
                                                                       PositiveInt seconds,
                                                                       PositiveInt milliseconds,
                                                                       TimeOffset offset)
        {
            // Arrange
            TimeExpression first = new(hours.Item, minutes.Item, seconds.Item, milliseconds.Item, offset);
            TimeExpression other = new(hours.Item, minutes.Item, seconds.Item, milliseconds.Item, offset);

            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                  .BeTrue();
            first.GetHashCode().Should()
                               .Be(other.GetHashCode(), $"two instances that are equal must have the same hashcode");
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Creating_a_TimeExpression_from_variables_obtained_from_deconstruction_of_a_TimeExpression_source_should_equal_the_TimeExpression_source(TimeExpression source)
        {
            // Arrange
            (int hours, int minutes, int seconds, int milliseconds, TimeOffset offset) = source;

            // Act
            TimeExpression actual = new(hours, minutes, seconds, milliseconds, offset);

            // Assert
            actual.Should()
                  .Be(source);
        }
    }
}

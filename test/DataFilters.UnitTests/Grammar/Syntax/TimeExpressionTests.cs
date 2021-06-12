
namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;

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

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(TimeExpression first, object other, bool expected, string reason)
        {
            _outputHelper.WriteLine($"First instance : {first}");
            _outputHelper.WriteLine($"Second instance : {other}");

            // Act
            bool actual = first.Equals(other);
            int actualHashCode = first.GetHashCode();

            // Assert
            actual.Should()
                .Be(expected, reason);
            if (expected)
            {
                actualHashCode.Should()
                    .Be(other?.GetHashCode(), reason);
            }
            else
            {
                actualHashCode.Should()
                    .NotBe(other?.GetHashCode(), reason);
            }
        }
    }
}

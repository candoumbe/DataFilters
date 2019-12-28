using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class TimeExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public TimeExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(TimeExpression).Should()
                                            .BeAssignableTo<FilterExpression>().And
                                            .Implement<IEquatable<TimeExpression>>().And
                                            .HaveConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int) }).And
                                            .HaveProperty<int>("Hours").And
                                            .HaveProperty<int>("Minutes").And
                                            .HaveProperty<int>("Seconds").And
                                            .HaveProperty<int>("Milliseconds");

        [Theory]
        [InlineData(1, 1, 1, -1)]
        [InlineData(1, 1, -1, 1)]
        [InlineData(1, -1, 1, 1)]
        [InlineData(-1, 1, 1, 1)]
        public void CtorThrowsArgumentException_When_Any_Input_Is_Negative(int hours, int minutes, int seconds, int milliseconds)
        {
            // Act
            Action ctor = () => new TimeExpression(hours, minutes, seconds, milliseconds);

            // Assert
            string expectedParamName = hours < 0
                ? nameof(hours)
                : minutes < 0
                    ? nameof(minutes)
                    : seconds < 0
                        ? nameof(seconds)
                        : nameof(milliseconds);
            ctor.Should()
                .ThrowExactly<ArgumentOutOfRangeException>()
                .Where(ex => ex.ParamName == expectedParamName);
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
                    TimeExpression instance = new TimeExpression(hours: 2050, minutes: 10, seconds: 14);

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
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(TimeExpression first, object second, bool expected, string reason)
        {
            // Act
            bool actual = first.Equals(second);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}

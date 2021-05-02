using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    [Feature(nameof(DataFilters.Grammar.Syntax))]
    public class DateTimeExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public DateTimeExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(DateTimeExpression).Should()
                                            .BeAssignableTo<FilterExpression>().And
                                            .Implement<IEquatable<DateTimeExpression>>().And
                                            .HaveConstructor(new[] { typeof(DateExpression), typeof(TimeExpression)}).And
                                            .HaveConstructor(new[] { typeof(TimeExpression) }).And
                                            .HaveConstructor(new[] { typeof(DateExpression) }).And
                                            .HaveProperty<DateExpression>("Date").And
                                            .HaveProperty<TimeExpression>("Time");

        [Fact]
        public void Given_date_and_time_parameters_are_null_Constructor_should_throws_ArgumentException()
        {
            // Act
            Action ctor = () => new DateTimeExpression(null, null);

            // Assert
            ctor.Should()
                .ThrowExactly<ArgumentException>("both date and time are null");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                {
                    yield return new object[]
                    {
                        new DateTimeExpression(new DateExpression(year: 2012, month: 3, day: 1)),
                        null,
                        false,
                        "Comparing instance to null"
                    };

                    yield return new object[]
                    {
                        new DateTimeExpression(new DateExpression(year: 2012, month: 3, day: 1)),
                        new DateTimeExpression(new DateExpression(year: 2012, month: 3, day: 1)),
                        true,
                        "Comparing two instances with same dates"
                    };

                    yield return new object[]
                    {
                        new DateTimeExpression(new TimeExpression(hours: 10, minutes: 3)),
                        new DateTimeExpression(new DateExpression(year: 2012, month: 3, day: 1)),
                        false,
                        "Comparing two instances with different date and time"
                    };

                    yield return new object[]
                    {
                        new DateTimeExpression(date: new DateExpression(year: 2012, month: 3, day: 1), time: new TimeExpression(hours: 10, minutes: 3)),
                        new DateTimeExpression(date: new DateExpression(year: 2012, month: 3, day: 1), time: new TimeExpression(hours: 10, minutes: 3)),
                        true,
                        "Comparing two instances with same date and time"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(DateTimeExpression first, object other, bool expected, string reason)
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

        public static IEnumerable<object[]> DeconstructCases
        {
            get
            {
                yield return new object[]
                {
                    new DateTimeExpression(new DateExpression(year: 1983, month: 6, day: 23)),
                    new DateExpression(year: 1983, month: 6, day: 23),
                    null
                };

                yield return new object[]
                {
                    new DateTimeExpression(new TimeExpression(hours: 12, minutes: 25, seconds: 46)),
                    null,
                    new TimeExpression(hours: 12, minutes: 25, seconds: 46),
                };
            }
        }

        [Theory]
        [MemberData(nameof(DeconstructCases))]
        public void DeconstructTests(DateTimeExpression dateTime, DateExpression expectedDate, TimeExpression expectedTime)
        {
            _outputHelper.WriteLine($"DateTimeExpression is {dateTime}");

            // Act
            (DateExpression date, TimeExpression time) = dateTime;

            // Assert
            date.Should()
                .Be(expectedDate);
            time.Should()
                .Be(expectedTime);
        }
    }
}

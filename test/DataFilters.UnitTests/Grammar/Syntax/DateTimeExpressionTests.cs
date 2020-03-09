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
        public void CtorThrowsArgumentException_When_Date_And_Time_Are_Null()
        {
            // Act
            Action ctor = () => new DateTimeExpression(null, null);

            // Assert
            ctor.Should()
                .ThrowExactly<ArgumentNullException>("both date and time are null");
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
        public void ImplementsEqualsCorrectly(DateTimeExpression first, object second, bool expected, string reason)
        {
            _outputHelper.WriteLine($"First instance : {first}");
            _outputHelper.WriteLine($"Second instance : {second}");

            // Act
            bool actual = first.Equals(second);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}

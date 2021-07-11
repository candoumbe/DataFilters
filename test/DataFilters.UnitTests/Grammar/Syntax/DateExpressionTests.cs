namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using FluentAssertions;
    using FsCheck.Xunit;
    using FsCheck;

    using System;
    using System.Collections.Generic;
    using Xunit;
    using Xunit.Abstractions;
    using DataFilters.UnitTests.Helpers;

    public class DateExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public DateExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(DateExpression).Should()
                                            .BeAssignableTo<FilterExpression>().And
                                            .Implement<IEquatable<DateExpression>>().And
                                            .HaveConstructor(new[] { typeof(int), typeof(int), typeof(int) }).And
                                            .HaveProperty<int>("Year").And
                                            .HaveProperty<int>("Month").And
                                            .HaveProperty<int>("Day");

        [Theory]
        [InlineData(1, 1, 0)]
        [InlineData(1, 0, 1)]
        [InlineData(0, 1, 1)]
        public void CtorThrowsArgumentException_When_Any_Input_Is_Less_Than_1(int year, int month, int day)
        {
            // Act
            Action ctor = () => new DateExpression(year, month, day);

            // Assert
            string expectedParamName;
            if (year < 1)
            {
                expectedParamName = nameof(year);
            }
            else if (month < 1)
            {
                expectedParamName = nameof(month);
            }
            else
            {
                expectedParamName = nameof(day);
            }

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
                    new DateExpression(year: 2019, month:1, day: 11),
                    new DateExpression(year: 2019, month:1, day: 11),
                    true,
                    $"comparing two {nameof(DateExpression)} instance with same value for each field"
                };
                {
                    DateExpression instance = new(year: 2050, month: 10, day: 14);

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
                        new DateExpression(instance.Year, instance.Month, instance.Day - 1),
                        false,
                        $"comparing two {nameof(DateExpression)} instance with that differs only by the {nameof(DateExpression.Day)} value"
                    };

                    yield return new object[]
                    {
                        instance,
                        new DateExpression(instance.Year, instance.Month - 1, instance.Day),
                        false,
                        $"comparing two {nameof(DateExpression)} instance with that differs only by the {nameof(DateExpression.Month)} value"
                    };

                    yield return new object[]
                    {
                        instance,
                        new DateExpression(instance.Year - 1, instance.Month, instance.Day),
                        false,
                        $"comparing two {nameof(DateExpression)} instance with that differs only by the {nameof(DateExpression.Year)} value"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(DateExpression first, object other, bool expected, string reason)
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

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_DateExpression_GetComplexity_should_return_1(DateExpression date) => (date.Complexity == 1).ToProperty();
    }
}

﻿namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Exceptions;
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;
    using FluentAssertions.Extensions;

    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;

    using System;
    using System.Collections.Generic;

    using Xunit;
    using Xunit.Abstractions;

    public class IntervalExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public IntervalExpressionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void IsFilterExpression() => typeof(IntervalExpression).Should()
                                                                   .BeAssignableTo<FilterExpression>().And
                                                                   .Implement<IEquatable<IntervalExpression>>().And
                                                                   .Implement<ISimplifiable>().And
                                                                   .HaveConstructor(new[] { typeof(BoundaryExpression), typeof(BoundaryExpression) }).And
                                                                   .HaveProperty<BoundaryExpression>("Min").And
                                                                   .HaveProperty<BoundaryExpression>("Max")
            ;

        [Fact]
        public void Given_min_and_max_are_null_Ctor_should_throws_IncorrectBoundaryException()
        {
            // Act
            Action action = () => new IntervalExpression(null, null);

            // Assert
            action.Should()
                .ThrowExactly<IncorrectBoundaryException>($"Either {nameof(IntervalExpression.Min)}/{nameof(IntervalExpression.Max)} must not be null");
        }

        public static IEnumerable<object[]> IncorrectBoundariesCases
        {
            get
            {
                yield return new object[] {
                    new BoundaryExpression(new AsteriskExpression(), included: true),
                    new BoundaryExpression(new AsteriskExpression(), included: true),
                    $"min and max cannot both be {nameof(AsteriskExpression)} instances"
                };

                yield return new object[] {
                    new BoundaryExpression(new AsteriskExpression(), included : true),
                    null,
                    $"max cannot be null when min is {nameof(AsteriskExpression)} instance"
                };
            }
        }

        public static IEnumerable<object[]> BoundariesTypeMismatchCases
        {
            get
            {
                yield return new object[] {
                    new BoundaryExpression(new DateExpression(), included: true), new BoundaryExpression(new NumericValueExpression("10"), included: true),
                    $"min holds {nameof(DateExpression)} and max holds {nameof(ConstantValueExpression)}"
                };

                yield return new object[] {
                    new BoundaryExpression(new NumericValueExpression("10"), included : true), new BoundaryExpression(new DateExpression(), included : true),
                    $"min holds {nameof(ConstantValueExpression)} and max holds {nameof(DateExpression)}"
                };

                yield return new object[]
                {
                    new BoundaryExpression(new TimeExpression(), included: true),
                    new BoundaryExpression(new DateExpression(), included: true),
                    $"min holds  { nameof(TimeExpression) } and max holds { nameof(DateExpression) }"
                };
            }
        }

        [Theory]
        [MemberData(nameof(IncorrectBoundariesCases))]
        public void Given_incorrect_boundaries_Ctor_should_throws_IncorrectBoundaryException(BoundaryExpression min, BoundaryExpression max, string reason)
        {
            // Act
            Action action = () => new IntervalExpression(min, max);

            // Assert
            action.Should()
                .ThrowExactly<IncorrectBoundaryException>(reason);
        }

        [Theory]
        [MemberData(nameof(BoundariesTypeMismatchCases))]
        public void Given_boundaries_that_are_not_compatible_Ctor_should_throws_BoundaryTypeMismatchException(BoundaryExpression min, BoundaryExpression max, string reason)
        {
            // Act
            Action action = () => new IntervalExpression(min, max);

            // Assert
            action.Should()
                .ThrowExactly<BoundariesTypeMismatchException>(reason)
                .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName));
        }

        public static IEnumerable<object[]> ValidCtorCases
        {
            get
            {
                yield return new object[]
                {
                    new BoundaryExpression(new DateExpression(), included: false),
                    new BoundaryExpression(new TimeExpression(), included: false)
                };

                yield return new object[]
                {
                    new BoundaryExpression(new AsteriskExpression(), included: false),
                    new BoundaryExpression(new TimeExpression(), included: false)
                };

                yield return new object[]
                {
                    new BoundaryExpression(new TimeExpression(), included: false),
                    new BoundaryExpression(new AsteriskExpression(), included: false)
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidCtorCases))]
        public void Given_valid_min_and_max_boundaries_Ctor_should_not_throws(BoundaryExpression min, BoundaryExpression max)
        {
            // Act
            Action ctor = () => new IntervalExpression(min, max);

            ctor.Should()
                .NotThrow();
        }

        public static IEnumerable<object[]> CtorLogicCases
        {
            get
            {
                DateTime dateTime = 12.March(2019);
                yield return new object[]
                {
                    new BoundaryExpression(new DateTimeExpression(dateTime), included : true),
                    new BoundaryExpression(new TimeExpression(hours: 10), included : true),
                    new IntervalExpression(min: new BoundaryExpression(new DateTimeExpression(dateTime), included : true),
                                           max: new BoundaryExpression(new DateTimeExpression(dateTime.Add(10.Hours())), included: true)),
                    "max date part should be deduced from min date part when max date part is not specified"
                };
            }
        }

        [Theory(DisplayName = "Constructor Logic")]
        [MemberData(nameof(CtorLogicCases))]
        public void CtorLogic(BoundaryExpression min, BoundaryExpression max, IntervalExpression expected, string reason)
        {
            // Act
            IntervalExpression actual = new(min, max);

            // Assert
            actual.Should()
                  .Be(expected, reason);
        }

        public static IEnumerable<object[]> EqualCases
        {
            get
            {
                yield return new object[]
                {
                    new IntervalExpression(min: new BoundaryExpression(new NumericValueExpression("10"), included : true)),
                    new IntervalExpression(min: new BoundaryExpression(new NumericValueExpression("10"), included : true)),
                    true,
                    $"comparing two {nameof(IntervalExpression)} instances with same min and max"
                };

                yield return new object[]
                {
                    new IntervalExpression(min: new BoundaryExpression(new NumericValueExpression("10"), included : true)),
                    new IntervalExpression(min: new BoundaryExpression(new NumericValueExpression("10"), included : false)),
                    false,
                    $"comparing two {nameof(IntervalExpression)} instances with same min and max but not same {nameof(BoundaryExpression.Included)}"
                };

                yield return new object[]
                {
                    new IntervalExpression(min: new BoundaryExpression(new NumericValueExpression("10"), included : true)),
                    null,
                    false,
                    "comparing to null"
                };

                yield return new object[]
                {
                    new IntervalExpression(max: new BoundaryExpression(new NumericValueExpression("10"), included : true)),
                    new IntervalExpression(max: new BoundaryExpression(new NumericValueExpression("10"), included : true)),
                    true,
                    $"comparing two {nameof(IntervalExpression)} instances with same min and max"
                };

                yield return new object[]
                {
                    new IntervalExpression(max: new BoundaryExpression( new DateTimeExpression(new DateExpression()), included : true)),
                    new IntervalExpression(max: new BoundaryExpression( new DateTimeExpression(new DateExpression()), included : true)),
                    true,
                    $"comparing two {nameof(IntervalExpression)} instances with same {nameof(DateTimeExpression)} min and max"
                };

                {
                    IntervalExpression interval = new(min: new BoundaryExpression(new DateTimeExpression(new DateExpression(2012, 10, 19), new TimeExpression(15,03, 45), OffsetExpression.Zero), included: false),
                                                max: new BoundaryExpression(new DateTimeExpression(new DateExpression(2012, 10, 19), new TimeExpression(15, 03, 45), new (hours: 1)), included: false));
                    yield return new object[]
                    {
                        interval,
                        interval,
                        true,
                        "Comparing two ranges"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualCases))]
        public void TestEquals(IntervalExpression first, object other, bool expected, string reason)
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
        public void Ctor_should_converts_Min_DateTimeExpression_to_DateExpression_when_TimeExpression_is_not_provided(DateExpression date, bool included)
        {
            // Arrange
            DateTimeExpression dateTimeExpression = new(date);
            BoundaryExpression minBoundary = new(dateTimeExpression, included);

            // Act
            IntervalExpression sut = new(minBoundary);

            // Assert
            sut.Min.Expression.Should().Be(date);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_Min_boundary_is_a_DateTimeExpression_with_only_time_specified_Ctor_should_convert_min_boundary_to_only_holds_the_specified_TimeExpression(NonNull<TimeExpression> time, bool included)
        {
            // Arrange
            DateTimeExpression dateTimeExpression = new(time.Item);

            // Act
            IntervalExpression interval = new(min: new BoundaryExpression(dateTimeExpression, included));

            return (interval.Min.Expression is TimeExpression timeExpression && timeExpression.Equals(time.Item)).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_Min_boundary_is_a_DateTimeExpression_with_only_date_specified_Ctor_should_convert_min_boundary_to_only_holds_the_specified_DateExpression(NonNull<DateExpression> date, bool included)
        {
            // Arrange
            // Arrange
            DateTimeExpression dateTimeExpression = new(date.Item);

            // Act
            IntervalExpression interval = new(min: new BoundaryExpression(dateTimeExpression, included));

            return (interval.Min.Expression is DateExpression dateExpression && dateExpression.Equals(date.Item)).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Ctor_should_converts_Max_DateTimeExpression_to_TimeExpression_when_DateExpression_is_not_provided(PositiveInt hours,
                                                                                                                      PositiveInt minutes,
                                                                                                                      PositiveInt seconds,
                                                                                                                      PositiveInt milliseconds,
                                                                                                                      bool included)
        {
            // Arrange
            TimeExpression time = new(hours.Item, minutes.Item, seconds.Item, milliseconds.Item);
            DateTimeExpression dateTimeExpression = new(time);

            // Act
            IntervalExpression interval = new(max: new BoundaryExpression(dateTimeExpression, included));
            (IBoundaryExpression expression, _) = interval.Max;

            // Assert
            expression.Should()
                      .Be(time);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_a_ConstantExpression_that_is_equal_to_min_and_min_and_max_are_equal_and_min_and_max_are_included_IsEquivalent_should_return_true_when_comparing_with_ConstantExpression(ConstantValueExpression constant, bool minIsIncluded, bool maxIsIncluded)
        {
            return CreateIsEquivalentPropeprty(constant, constant, minIsIncluded, maxIsIncluded).When(minIsIncluded && maxIsIncluded);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_a_DateExpression_that_is_equal_to_min_and_min_and_max_are_equal_and_min_and_max_are_included_IsEquivalent_should_return_true_when_comparing_with_DateExpression(DateExpression date, bool minIsIncluded, bool maxIsIncluded)
        {
            return CreateIsEquivalentPropeprty(date, date, minIsIncluded, maxIsIncluded).When(minIsIncluded && maxIsIncluded);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_a_DateTimeExpression_that_is_equal_to_min_and_min_and_max_are_equal_and_min_and_max_are_included_IsEquivalent_should_return_true_when_comparing_with_DateTimeExpression(DateTimeExpression dateTime, bool minIsIncluded, bool maxIsIncluded)
        {
            return CreateIsEquivalentPropeprty(dateTime, dateTime, minIsIncluded, maxIsIncluded).When(minIsIncluded && maxIsIncluded);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_a_DateTimeExpression_that_is_equal_to_min_and_min_and_max_are_equal_and_min_and_max_are_included_IsEquivalent_should_return_true_when_comparing_with_TimeExpression(TimeExpression dateTime, bool minIsIncluded, bool maxIsIncluded)
        {
            return CreateIsEquivalentPropeprty(dateTime, dateTime, minIsIncluded, maxIsIncluded).When(minIsIncluded && maxIsIncluded);
        }

        private static Property CreateIsEquivalentPropeprty(FilterExpression filterExpression, IBoundaryExpression boundaryExpression, bool minIncluded, bool maxIsIncluded)
        {
            // Arrange
            IntervalExpression range = new(new(boundaryExpression, minIncluded), new(boundaryExpression, maxIsIncluded));

            return range.IsEquivalentTo(filterExpression).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_RangeExpression_GetComplexity_should_return_sum_of_min_and_max_complexity(IntervalExpression rangeExpression)
            => (rangeExpression.Complexity == (rangeExpression.Min?.Expression?.Complexity ?? 0) + (rangeExpression.Max?.Expression?.Complexity ?? 0)).ToProperty();

        public static IEnumerable<object[]> SimplifyCases
        {
            get
            {
                yield return new object[]
                {
                    new IntervalExpression(new (new NumericValueExpression("10"), true), new (new NumericValueExpression("10"), true)),
                    new NumericValueExpression("10"),
                    "Lower and upper bound are equal"
                };

                yield return new object[]
                {
                    new IntervalExpression(new (new NumericValueExpression("10"), true), new (new NumericValueExpression("12"), true)),
                    new IntervalExpression(new (new NumericValueExpression("10"), true), new (new NumericValueExpression("12"), true)),
                    "Lower and upper bound are not equals and not equivalent"
                };

                yield return new object[]
                {
                    new IntervalExpression(new (new TimeExpression(1), true), new (new TimeExpression(minutes: 60), true)),
                    new TimeExpression(1),
                    "Lower and upper bound are equivalent"
                };
            }
        }

        [Theory]
        [MemberData(nameof(SimplifyCases))]
        public void Given_RangeExpression_Simplify_should_return_expected_expression(IntervalExpression rangeExpression, FilterExpression expected, string reason)
        {
            _outputHelper.WriteLine($"Range expression : {rangeExpression}");

            // Act
            FilterExpression actual = rangeExpression.Simplify();

            // Assert
            actual.Should()
                  .Be(expected, reason);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void An_IntervalExpression_instance_built_from_data_from_a_previously_deconstructed_instance_should_be_equal(IntervalExpression expected)
        {
            // Arrange
            (BoundaryExpression min, BoundaryExpression max) = expected;

            // Act
            IntervalExpression actual = new(min, max);

            // Assert
            actual.Should()
                  .Be(expected);
        }
    }
}
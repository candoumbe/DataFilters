using DataFilters.Grammar.Exceptions;
using DataFilters.Grammar.Syntax;
using DataFilters.UnitTests.Helpers;
using FluentAssertions;
using FluentAssertions.Extensions;
using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class RangeExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public RangeExpressionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void IsFilterExpression() => typeof(RangeExpression).Should()
                                                                   .BeAssignableTo<FilterExpression>().And
                                                                   .Implement<IEquatable<RangeExpression>>().And
                                                                   .HaveConstructor(new[] { typeof(BoundaryExpression), typeof(BoundaryExpression) }).And
                                                                   .HaveProperty<BoundaryExpression>("Min").And
                                                                   .HaveProperty<BoundaryExpression>("Max")
            ;

        [Fact]
        public void Given_min_and_max_are_null_Ctor_should_throws_IncorrectBoundaryException()
        {
            // Act
            Action action = () => new RangeExpression(null, null);

            // Assert
            action.Should()
                .ThrowExactly<IncorrectBoundaryException>($"Either {nameof(RangeExpression.Min)}/{nameof(RangeExpression.Max)} must not be null");
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
                    new BoundaryExpression(new DateExpression(), included: true), new BoundaryExpression(new ConstantValueExpression("10"), included: true),
                    $"min holds {nameof(DateExpression)} and max holds {nameof(ConstantValueExpression)}"
                };

                yield return new object[] {
                    new BoundaryExpression(new ConstantValueExpression("10"), included : true), new BoundaryExpression(new DateExpression(), included : true),
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
            Action action = () => new RangeExpression(min, max);

            // Assert
            action.Should()
                .ThrowExactly<IncorrectBoundaryException>(reason);
        }

        [Theory]
        [MemberData(nameof(BoundariesTypeMismatchCases))]
        public void Given_boundaries_that_are_not_compatible_Ctor_should_throws_BoundaryTypeMismatchException(BoundaryExpression min, BoundaryExpression max, string reason)
        {
            // Act
            Action action = () => new RangeExpression(min, max);

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
            Action ctor = () => new RangeExpression(min, max);

            ctor.Should()
                .NotThrow();
        }

        public static IEnumerable<object[]> CtorLogicCases
        {
            get
            {
                {
                    DateTime dateTime = 12.March(2019);
                    yield return new object[]
                    {
                        new BoundaryExpression(new DateTimeExpression(dateTime), included : true),
                        new BoundaryExpression(new TimeExpression(hours: 10), included : true),
                        new RangeExpression(min: new BoundaryExpression(new DateTimeExpression(dateTime), included : true),
                                            max: new BoundaryExpression(new DateTimeExpression(dateTime.AddHours(10)), included: true)),
                        "max date part should be deduced from min date part when max date part is not specified"
                    };
                }
            }
        }

        [Theory(DisplayName = "Constructor Logic")]
        [MemberData(nameof(CtorLogicCases))]
        public void CtorLogic(BoundaryExpression min, BoundaryExpression max, RangeExpression expected, string reason)
        {
            // Act
            RangeExpression actual = new(min, max);

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
                    new RangeExpression(min: new BoundaryExpression(new ConstantValueExpression("10"), included : true)),
                    new RangeExpression(min: new BoundaryExpression(new ConstantValueExpression("10"), included : true)),
                    true,
                    $"comparing two {nameof(RangeExpression)} instances with same min and max"
                };

                yield return new object[]
                {
                    new RangeExpression(min: new BoundaryExpression(new ConstantValueExpression("10"), included : true)),
                    new RangeExpression(min: new BoundaryExpression(new ConstantValueExpression("10"), included : false)),
                    false,
                    $"comparing two {nameof(RangeExpression)} instances with same min and max but not same {nameof(BoundaryExpression.Included)}"
                };

                yield return new object[]
                {
                    new RangeExpression(min: new BoundaryExpression(new ConstantValueExpression("10"), included : true)),
                    null,
                    false,
                    "comparing to null"
                };

                yield return new object[]
                {
                    new RangeExpression(max: new BoundaryExpression(new ConstantValueExpression("10"), included : true)),
                    new RangeExpression(max: new BoundaryExpression(new ConstantValueExpression("10"), included : true)),
                    true,
                    $"comparing two {nameof(RangeExpression)} instances with same min and max"
                };

                yield return new object[]
                {
                    new RangeExpression(max: new BoundaryExpression( new DateTimeExpression(new DateExpression()), included : true)),
                    new RangeExpression(max: new BoundaryExpression( new DateTimeExpression(new DateExpression()), included : true)),
                    true,
                    $"comparing two {nameof(RangeExpression)} instances with same {nameof(DateTimeExpression)} min and max"
                };

                {
                    RangeExpression range = new(min: new BoundaryExpression(new ConstantValueExpression("2012-10-19T15:03:45Z"), included: false),
                                                max: new BoundaryExpression(new ConstantValueExpression("2012-10-19T15:30:45+01:00"), included: false));
                    yield return new object[]
                    {
                        range,
                        range,
                        true,
                        "Comparing two ranges"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualCases))]
        public void TestEquals(RangeExpression first, object other, bool expected, string reason)
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
        public Property Ctor_should_converts_Min_DateTimeExpression_to_DateExpression_when_TimeExpression_is_not_provided(DateExpression date, bool included)
        {
            // Arrange
            Lazy<RangeExpression> lazyRangeExpression = new(() =>
            {
                DateTimeExpression dateTimeExpression = new(date);

                return new(min: new BoundaryExpression(dateTimeExpression, included));
            });

            return Prop.Throws<ArgumentOutOfRangeException, RangeExpression>(lazyRangeExpression).When(date.Day < 0 || date.Month < 0 || date.Year < 0)
                .Or(Prop.Throws<ArgumentNullException, RangeExpression>(lazyRangeExpression).When(date is null))
                .Or(lazyRangeExpression.Value.Min.Expression is DateExpression dateExpression && dateExpression.Equals(date));
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Ctor_should_converts_Min_DateTimeExpression_to_TimeExpression_when_DateExpression_is_not_provided(TimeExpression time, bool included)
        {
            // Arrange
            Lazy<RangeExpression> lazyRangeExpression = new(() =>
            {
                DateTimeExpression dateTimeExpression = new(time);

                return new(min: new BoundaryExpression(dateTimeExpression, included));
            });

            return Prop.Throws<ArgumentOutOfRangeException, RangeExpression>(lazyRangeExpression).When(time.Hours < 0 || time.Minutes < 0 || time.Seconds < 0)
                .Or(Prop.Throws<ArgumentNullException, RangeExpression>(lazyRangeExpression).When(time is null))
                    .Or(lazyRangeExpression.Value.Min.Expression is TimeExpression timeExpression && timeExpression.Equals(time));
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Ctor_should_converts_Max_DateTimeExpression_to_DateExpression_when_TimeExpression_is_not_provided(DateExpression date, bool included)
        {
            // Arrange
            Lazy<RangeExpression> lazyRangeExpression = new(() =>
            {
                DateTimeExpression dateTimeExpression = new(date);

                return new(max: new BoundaryExpression(dateTimeExpression, included));
            });

            return Prop.Throws<ArgumentOutOfRangeException, RangeExpression>(lazyRangeExpression).When(date.Day < 0 || date.Month < 0 || date.Year < 0)
                    .Or(Prop.Throws<ArgumentNullException, RangeExpression>(lazyRangeExpression).When(date is null))
                    .Or(lazyRangeExpression.Value.Max.Expression is DateExpression dateExpression && dateExpression.Equals(date));
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Ctor_should_converts_Max_DateTimeExpression_to_TimeExpression_when_DateExpression_is_not_provided(TimeExpression time, bool included)
        {
            // Arrange
            Lazy<RangeExpression> lazyRangeExpression = new(() =>
            {
                DateTimeExpression dateTimeExpression = new(time);

                return new(max: new BoundaryExpression(dateTimeExpression, included));
            });

            return Prop.Throws<ArgumentOutOfRangeException, RangeExpression>(lazyRangeExpression).When(time.Hours < 0 || time.Minutes < 0 || time.Seconds < 0)
                    .Or(Prop.Throws<ArgumentNullException, RangeExpression>(lazyRangeExpression).When(time is null))
                .Or(lazyRangeExpression.Value.Max.Expression is TimeExpression timeExpression && timeExpression.Equals(time));
        }
    }
}

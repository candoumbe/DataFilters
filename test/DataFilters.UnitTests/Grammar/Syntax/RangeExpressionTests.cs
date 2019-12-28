using DataFilters.Grammar.Exceptions;
using DataFilters.Grammar.Syntax;
using FluentAssertions;
using FluentAssertions.Extensions;
using System;
using System.Collections.Generic;
using Xunit;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class RangeExpressionTests
    {
        [Fact]
        public void IsFilterExpression() => typeof(RangeExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<RangeExpression>>().And
            .HaveConstructor(new[] { typeof(IBoundaryExpression), typeof(IBoundaryExpression) }).And
            .HaveProperty<IBoundaryExpression>("Min").And
            .HaveProperty<IBoundaryExpression>("Max")
            ;

        [Fact]
        public void Ctor_Throws_ArgumentNullException_If_Value_Is_Null()
        {
            // Act
            Action action = () => new RangeExpression(null, null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"Either {nameof(RangeExpression.Min)}/{nameof(RangeExpression.Max)} must be set");
        }

        public static IEnumerable<object[]> IncorrectBoundariesCases
        {
            get
            {
                yield return new object[] {
                    new AsteriskExpression(), new AsteriskExpression(),
                    $"min and max cannot both be {nameof(AsteriskExpression)} instances"
                };

                yield return new object[] {
                    new AsteriskExpression(), null,
                    $"max cannot be null when min is {nameof(AsteriskExpression)} instance"
                };
            }
        }

        public static IEnumerable<object[]> BoundariesTypeMismatchCases
        {
            get
            {
                yield return new object[] {
                    new DateExpression(), new ConstantExpression("10"),
                    $"min is {nameof(DateExpression)} and max is {nameof(ConstantExpression)}"
                };

                yield return new object[] {
                    new ConstantExpression("10"), new DateExpression(),
                    $"min is {nameof(ConstantExpression)} and max is {nameof(DateExpression)}"
                };

                yield return new object[] {
                    new TimeExpression(), new DateExpression(),
                    $"min is {nameof(TimeExpression)} and max is {nameof(DateExpression)}"
                };
            }
        }

        [Theory]
        [MemberData(nameof(IncorrectBoundariesCases))]
        public void Ctor_Throws_IncorrectBoundaryException_If_Boundaries_Are_Incoherent(IBoundaryExpression min, IBoundaryExpression max, string reason)
        {
            // Act
            Action action = () => new RangeExpression(min, max);

            // Assert
            action.Should()
                .ThrowExactly<IncorrectBoundaryException>(reason)
                .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName));
        }

        [Theory]
        [MemberData(nameof(BoundariesTypeMismatchCases))]
        public void Ctor_Throws_BoundaryTypeMismatchException_If_Boundaries_Type_Are_Not_TheSame(IBoundaryExpression min, IBoundaryExpression max, string reason)
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
                    new DateExpression(), new TimeExpression()
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidCtorCases))]
        public void Ctor_DoesNot_Throws(IBoundaryExpression min, IBoundaryExpression max)
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
                        new DateTimeExpression(dateTime), new TimeExpression(hours: 10),
                        new RangeExpression(min: new DateTimeExpression(dateTime), max : new DateTimeExpression(dateTime.AddHours(10))),
                        "max date part should be deduced from min date part when max date part is not specified"
                    };
                }
            }
        }

        [Theory(DisplayName = "Constructor Logic")]
        [MemberData(nameof(CtorLogicCases))]
        public void CtorLogic(IBoundaryExpression min, IBoundaryExpression max, RangeExpression expected, string reason)
        {
            // Act
            RangeExpression actual = new RangeExpression(min, max);

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
                    new RangeExpression(min: new ConstantExpression("10")),
                    new RangeExpression(min: new ConstantExpression("10")),
                    true,
                    $"comparing two {nameof(RangeExpression)} instances with same min and max"
                };

                yield return new object[]
                {
                    new RangeExpression(max: new ConstantExpression("10")),
                    new RangeExpression(max: new ConstantExpression("10")),
                    true,
                    $"comparing two {nameof(RangeExpression)} instances with same min and max"
                };

                yield return new object[]
                {
                    new RangeExpression(max: new DateTimeExpression(new DateExpression())),
                    new RangeExpression(max: new DateTimeExpression(new DateExpression())),
                    true,
                    $"comparing two {nameof(RangeExpression)} instances with same {nameof(DateTimeExpression)} min and max"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualCases))]
        public void TestEquals(RangeExpression first, object second, bool expected, string reason)
        {
            // Act
            bool actual = first.Equals(second);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}

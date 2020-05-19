using DataFilters.Grammar.Exceptions;
using DataFilters.Grammar.Syntax;

using FluentAssertions;
using FluentAssertions.Extensions;

using System;
using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

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
                    new BoundaryExpression(new DateExpression(), included: true), new BoundaryExpression(new ConstantExpression("10"), included: true),
                    $"min holds {nameof(DateExpression)} and max holds {nameof(ConstantExpression)}"
                };

                yield return new object[] {
                    new BoundaryExpression(new ConstantExpression("10"), included : true), new BoundaryExpression(new DateExpression(), included : true),
                    $"min holds {nameof(ConstantExpression)} and max holds {nameof(DateExpression)}"
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
        public void Ctor_throws_IncorrectBoundaryException_if_boundaries_are_incoherent(BoundaryExpression min, BoundaryExpression max, string reason)
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
        public void Ctor_throws_BoundaryTypeMismatchException_if_boundaries_type_are_not_compatible(BoundaryExpression min, BoundaryExpression max, string reason)
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
                    new BoundaryExpression(new DateExpression(), included: false), new BoundaryExpression(new TimeExpression(), included: false)
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidCtorCases))]
        public void Ctor_does_not_throws(BoundaryExpression min, BoundaryExpression max)
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
                    new RangeExpression(min: new BoundaryExpression(new ConstantExpression("10"), included : true)),
                    new RangeExpression(min: new BoundaryExpression(new ConstantExpression("10"), included : true)),
                    true,
                    $"comparing two {nameof(RangeExpression)} instances with same min and max"
                };

                yield return new object[]
                {
                    new RangeExpression(min: new BoundaryExpression(new ConstantExpression("10"), included : true)),
                    new RangeExpression(min: new BoundaryExpression(new ConstantExpression("10"), included : false)),
                    false,
                    $"comparing two {nameof(RangeExpression)} instances with same min and max but not same {nameof(BoundaryExpression.Included)}"
                };

                yield return new object[]
                {
                    new RangeExpression(min: new BoundaryExpression(new ConstantExpression("10"), included : true)),
                    null,
                    false,
                    "comparing to null"
                };

                yield return new object[]
                {
                    new RangeExpression(max: new BoundaryExpression(new ConstantExpression("10"), included : true)),
                    new RangeExpression(max: new BoundaryExpression(new ConstantExpression("10"), included : true)),
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
    }
}

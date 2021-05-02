using DataFilters.TestObjects;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using static DataFilters.SortDirection;

namespace DataFilters.UnitTests
{
    public class SortTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public SortTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Theory]
        [InlineData("", "Expression is empty")]
        [InlineData(" ", "Expression is whitespace only")]
        [InlineData(null, "Expression is null")]
        public void Ctor_Throws_ArgumentException_If_Expression_Is_Null(string expression, string reason)
        {
            // Act
            Action action = () => new Sort<object>(expression);

            // Assert
            action.Should()
                .Throw<ArgumentException>(reason)
                .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName), "Param name cannot be null")
                .Where(ex => !string.IsNullOrWhiteSpace(ex.Message), "Message cannot be null");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new Sort<SuperHero>("Name"),
                    new Sort<SuperHero>("Name"),
                    true,
                    $"Two distinct {nameof(Sort<SuperHero>)} instances with same properties must be equal"
                };

                Sort<SuperHero> sort = new("Name");
                yield return new object[]
                {
                    sort,
                    sort,
                    true,
                    $"A {nameof(Sort<SuperHero>)} instance is equal to itself"
                };

                yield return new object[]
                {
                    new Sort<SuperHero>("Name", Descending),
                    new Sort<SuperHero>("Name"),
                    false,
                    $"Two distinct {nameof(Sort<SuperHero>)} instances with same {nameof(Sort<SuperHero>.Expression)} but different {nameof(Sort<SuperHero>.Direction)} must not be equal"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void EqualsTests(Sort<SuperHero> first, object second, bool expected, string reason)
        {
            _outputHelper.WriteLine($"{nameof(first)} : '{first}'");
            _outputHelper.WriteLine($"{nameof(second)} : '{second}'");

            // Act
            bool actual = first.Equals(second);
            int actualHashCode = first.GetHashCode();

            // Assert
            actual.Should()
                .Be(expected, reason);

            if (expected)
            {
                actualHashCode.Should()
                    .Be(second?.GetHashCode(), reason);
            }
            else
            {
                actualHashCode.Should()
                    .NotBe(second?.GetHashCode(), reason);
            }
        }
    }
}

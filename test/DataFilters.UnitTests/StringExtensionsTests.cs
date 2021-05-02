using DataFilters.Casing;
using DataFilters.TestObjects;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using static DataFilters.SortDirection;

namespace DataFilters.UnitTests
{
    public class StringExtensionsTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public StringExtensionsTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Theory]
        [InlineData(null, "sort expression cannot be null")]
        [InlineData("  ", "sort expression cannot be whitespace only")]
        public void Throws_ArgumentNullException_When_Parameter_IsNull(string source, string reason)
        {
            // Act
            Action action = () => source.ToSort<SuperHero>();

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>(reason)
                .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName), $"{nameof(ArgumentOutOfRangeException.ParamName)} must not be null")
                .Where(ex => !string.IsNullOrWhiteSpace(ex.Message), $"{nameof(ArgumentOutOfRangeException.Message)} must not be null");
        }

        [Theory]
        [InlineData("prop1 prop2", "sort expression contains two properties that are not separated by a comma")]
        [InlineData("prop1,prop2,", "sort expression cannot ends with a comma")]
        [InlineData(",prop1,prop2", "sort expression cannot starts with a comma")]
        [InlineData("--prop", "sort expression can start with only one hyphen")]
        public void Throws_InvalidSortExpression_When_Expression_IsNotValid(string invalidExpression, string reason)
        {
            // Act
            Action action = () => invalidExpression.ToSort<SuperHero>();

            // Assert
            action.Should()
                .ThrowExactly<InvalidSortExpressionException>(reason)
                .Where(ex => !string.IsNullOrWhiteSpace(ex.Message), $"{nameof(ArgumentOutOfRangeException.Message)} must not be null");
        }

        public static IEnumerable<object[]> ToSortCases
        {
            get
            {
                yield return new object[]
                {
                    nameof(SuperHero.Nickname),
                    new Sort<SuperHero>(expression : nameof(SuperHero.Nickname), direction : Ascending)
                };
                yield return new object[]
                {
                    $"+{nameof(SuperHero.Nickname)}",
                    new Sort<SuperHero>(expression : nameof(SuperHero.Nickname), direction : Ascending)
                };

                yield return new object[]
                {
                    $"-{nameof(SuperHero.Nickname)}",
                    new Sort<SuperHero>(expression : nameof(SuperHero.Nickname), direction : Descending)
                };

                {
                    MultiSort<SuperHero> multiSort = new
                    (
                        new Sort<SuperHero>(expression: nameof(SuperHero.Nickname)),
                        new Sort<SuperHero>(expression: nameof(SuperHero.Age))
                    );

                    yield return new object[]
                    {
                        $"{nameof(SuperHero.Nickname)},{nameof(SuperHero.Age)}",
                        multiSort
                    };
                }
                {
                    MultiSort<SuperHero> multiSort = new
                    (
                        new Sort<SuperHero>(expression: nameof(SuperHero.Nickname)),
                        new Sort<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending)
                    );

                    yield return new object[]
                    {
                        $"+{nameof(SuperHero.Nickname)},-{nameof(SuperHero.Age)}",
                        multiSort
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ToSortCases))]
        public void ToSortTests(string sort, ISort<SuperHero> expected)
        {
            _outputHelper.WriteLine($"{nameof(sort)} : '{sort}'");

            // Act
            ISort<SuperHero> actual = sort.ToSort<SuperHero>();

            _outputHelper.WriteLine($"actual sort : '{actual}'");

            // Assert
            actual.Should()
                  .Be(expected);
        }

        public static IEnumerable<object[]> ToSortWithPropertyNameResolutionStrategyCases
        {
            get
            {
                yield return new object[]
                {
                    "-SnakeCaseProperty",
                    PropertyNameResolutionStrategy.SnakeCase,
                    new Sort<Model>("snake_case_property", Descending)
                };

                yield return new object[]
                {
                    "SnakeCaseProperty",
                    PropertyNameResolutionStrategy.SnakeCase,
                    new Sort<Model>("snake_case_property", Ascending)
                };

                yield return new object[]
                {
                    "-SnakeCaseProperty",
                    PropertyNameResolutionStrategy.SnakeCase,
                    new Sort<Model>("snake_case_property", Descending)
                };

                yield return new object[]
                {
                    "pascal_case_property",
                    PropertyNameResolutionStrategy.PascalCase,
                    new Sort<Model>("PascalCaseProperty", Ascending)
                };

                yield return new object[]
                {
                    "+pascal_case_property",
                    PropertyNameResolutionStrategy.PascalCase,
                    new Sort<Model>("PascalCaseProperty", Ascending)
                };

                yield return new object[]
                {
                    "-pascal_case_property",
                    PropertyNameResolutionStrategy.PascalCase,
                    new Sort<Model>("PascalCaseProperty", Descending)
                };
            }
        }

        [Theory]
        [MemberData(nameof(ToSortWithPropertyNameResolutionStrategyCases))]
        public void Given_input_and_PropertyNameResolutionStrategy_ToSort_should_return_appropriate_ISort_instance(string sort, PropertyNameResolutionStrategy propertyNameResolutionStrategy, ISort<Model> expected)
        {
            // Act
            ISort<Model> actual = sort.ToSort<Model>(propertyNameResolutionStrategy);

            // Assert
            actual.Should()
                  .Be(expected);

        }
    }
}

﻿namespace DataFilters.UnitTests
{
    using System;
    using DataFilters.Casing;
    using DataFilters.TestObjects;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using static DataFilters.OrderDirection;

    public class StringExtensionsTests(ITestOutputHelper outputHelper)
    {
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
        public void Given_invalid_expression_ToSort_throws_InvalidSortExpression(string invalidExpression, string reason)
        {
            // Act
            Action action = () => invalidExpression.ToSort<SuperHero>();

            // Assert
            action.Should()
                .ThrowExactly<InvalidOrderExpressionException>(reason)
                .Where(ex => !string.IsNullOrWhiteSpace(ex.Message), $"{nameof(ArgumentOutOfRangeException.Message)} must not be null");
        }

        public static TheoryData<string, IOrder<SuperHero>> ToSortCases
            => new()
            {
                {
                    nameof(SuperHero.Nickname),
                    new Order<SuperHero>(expression : nameof(SuperHero.Nickname), direction : Ascending)
                },
                {
                    $"+{nameof(SuperHero.Nickname)}",
                    new Order<SuperHero>(expression : nameof(SuperHero.Nickname), direction : Ascending)
                },
                {
                    $"-{nameof(SuperHero.Nickname)}",
                    new Order<SuperHero>(expression : nameof(SuperHero.Nickname), direction : Descending)
                },
                {
                    $"{nameof(SuperHero.Nickname)},{nameof(SuperHero.Age)}",
                    new MultiOrder<SuperHero>
                    (
                        new Order<SuperHero>(expression: nameof(SuperHero.Nickname)),
                        new Order<SuperHero>(expression: nameof(SuperHero.Age))
                    )
                },
                {
                    $"+{nameof(SuperHero.Nickname)},-{nameof(SuperHero.Age)}",
                    new MultiOrder<SuperHero>
                    (
                        new Order<SuperHero>(expression: nameof(SuperHero.Nickname)),
                        new Order<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending)
                    )
                }
            };

        [Theory]
        [MemberData(nameof(ToSortCases))]
        public void ToSortTests(string sort, IOrder<SuperHero> expected)
        {
            outputHelper.WriteLine($"{nameof(sort)} : '{sort}'");

            // Act
            IOrder<SuperHero> actual = sort.ToSort<SuperHero>();

            outputHelper.WriteLine($"actual sort : '{actual}'");

            // Assert
            actual.Should()
                  .Be(expected);
        }

        public static TheoryData<string, PropertyNameResolutionStrategy, IOrder<Model>> ToSortWithPropertyNameResolutionStrategyCases
            => new()
            {
                {
                    "-SnakeCaseProperty",
                    PropertyNameResolutionStrategy.SnakeCase,
                    new Order<Model>("snake_case_property", Descending)
                },
                {
                    "SnakeCaseProperty",
                    PropertyNameResolutionStrategy.SnakeCase,
                    new Order<Model>("snake_case_property", Ascending)
                },
                {
                    "-SnakeCaseProperty",
                    PropertyNameResolutionStrategy.SnakeCase,
                    new Order<Model>("snake_case_property", Descending)
                },
                {
                    "pascal_case_property",
                    PropertyNameResolutionStrategy.PascalCase,
                    new Order<Model>("PascalCaseProperty", Ascending)
                },
                {
                    "+pascal_case_property",
                    PropertyNameResolutionStrategy.PascalCase,
                    new Order<Model>("PascalCaseProperty", Ascending)
                },
                {
                    "-pascal_case_property",
                    PropertyNameResolutionStrategy.PascalCase,
                    new Order<Model>("PascalCaseProperty", Descending)
                }
            };

        [Theory]
        [MemberData(nameof(ToSortWithPropertyNameResolutionStrategyCases))]
        public void Given_input_and_PropertyNameResolutionStrategy_ToSort_should_return_appropriate_ISort_instance(string sort, PropertyNameResolutionStrategy propertyNameResolutionStrategy, IOrder<Model> expected)
        {
            // Act
            IOrder<Model> actual = sort.ToOrder<Model>(propertyNameResolutionStrategy);

            // Assert
            actual.Should()
                  .Be(expected);
        }
    }
}

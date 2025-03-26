namespace DataFilters.UnitTests;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using DataFilters.UnitTests.Helpers;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static DataFilters.FilterLogic;
using static DataFilters.FilterOperator;

[UnitTest]
public class FilterTests(ITestOutputHelper output)
{
    private static readonly IImmutableDictionary<string, FilterOperator> Operators = new Dictionary<string, FilterOperator>
    {
        ["contains"] = Contains,
        ["endswith"] = EndsWith,
        ["eq"] = EqualTo,
        ["gt"] = GreaterThan,
        ["gte"] = GreaterThanOrEqual,
        ["isempty"] = IsEmpty,
        ["isnotempty"] = IsNotEmpty,
        ["isnotnull"] = IsNotNull,
        ["isnull"] = IsNull,
        ["lt"] = FilterOperator.LessThan,
        ["lte"] = LessThanOrEqualTo,
        ["neq"] = NotEqualTo,
        ["startswith"] = StartsWith
    }.ToImmutableDictionary();

    /// <summary>
    /// Deserialization of various json representation into <see cref="Filter"/>
    /// </summary>
    public static TheoryData<string, Expression<Func<IFilter, bool>>> FilterDeserializeCases
    {
        get
        {
            TheoryData<string, Expression<Func<IFilter, bool>>> cases = [];
            foreach (KeyValuePair<string, FilterOperator> item in Operators)
            {
                cases.Add
                (
                    @$"{{ ""field"" : ""Firstname"", ""op"" : ""{item.Key}"",  ""Value"" : ""Batman""}}",
                    result => result is Filter
                              && "Firstname".Equals(((Filter)result).Field)
                              && item.Value.Equals(((Filter)result).Operator)
                              && "Batman".Equals(((Filter)result).Value)

                );
            }

            return cases;
        }
    }

    public static TheoryData<MultiFilter, Expression<Func<string, bool>>> CompositeFilterToJsonCases
        => new() {
            {
                new MultiFilter  {
                    Logic = Or,
                    Filters = new [] {
                        new Filter (field : "Nickname", @operator : EqualTo, value : "Batman"),
                        new Filter (field : "Nickname", @operator : EqualTo, value : "Robin")
                    }
                },
                json =>
                    JObject.Parse(json).Properties().Count() == 2

                    && "or".Equals((string) JObject.Parse(json)[MultiFilter.LogicJsonPropertyName])

                    && "Nickname".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][0][Filter.FieldJsonPropertyName])
                    && "eq".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][0][Filter.OperatorJsonPropertyName])
                    && "Batman".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][0][Filter.ValueJsonPropertyName])
                    && "Nickname".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][1][Filter.FieldJsonPropertyName])
                    && "eq".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][1][Filter.OperatorJsonPropertyName])
                    && "Robin".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][1][Filter.ValueJsonPropertyName])

            },
            {
                new MultiFilter  {
                    Filters = new [] {
                        new Filter (field : "Nickname", @operator : EqualTo, value : "Batman"),
                        new Filter (field : "Nickname", @operator : EqualTo, value : "Robin")
                    }
                },
                json =>
                    JObject.Parse(json).Properties().Count() == 2

                    && "and".Equals((string) JObject.Parse(json)[MultiFilter.LogicJsonPropertyName])

                    && "Nickname".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][0][Filter.FieldJsonPropertyName])
                    && "eq".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][0][Filter.OperatorJsonPropertyName])
                    && "Batman".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][0][Filter.ValueJsonPropertyName])
                    &&
                    "Nickname".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][1][Filter.FieldJsonPropertyName])
                    && "eq".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][1][Filter.OperatorJsonPropertyName])
                    && "Robin".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][1][Filter.ValueJsonPropertyName])

            }
        };

    /// <summary>
    /// Serialization of instance of <see cref="Filter"/> test cases
    /// </summary>
    public static TheoryData<Filter, Expression<Func<string, bool>>> FilterToJsonCases
        => new()
        {
            {
                new Filter (field : "Firstname", @operator  : EqualTo,  value : "Batman"),
                json =>
                    "Firstname".Equals((string) JObject.Parse(json)[Filter.FieldJsonPropertyName])
                    && "eq".Equals((string) JObject.Parse(json)[Filter.OperatorJsonPropertyName])
                    && "Batman".Equals((string) JObject.Parse(json)[Filter.ValueJsonPropertyName])

            }
        };
    [Theory]
    [MemberData(nameof(FilterToJsonCases))]
    public void FilterToJson(Filter filter, Expression<Func<string, bool>> jsonMatcher)
        => ToJson(filter, jsonMatcher);

    private void ToJson(IFilter filter, Expression<Func<string, bool>> jsonMatcher)
    {
        output.WriteLine($"Testing : {filter}{Environment.NewLine} against {Environment.NewLine} {jsonMatcher} ");

        // Act
        string json = filter.ToJson();

        // Assert
        output.WriteLine($"ToJson result is '{json}'");
        json.Should().Match(jsonMatcher);
    }

    public static TheoryData<string, FilterOperator, bool> FilterSchemaTestCases
        => new()
        {
            {
                $"{{{Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} :'batman' }}",
                EqualTo,
                true
            },

            {
                $"{{{Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : null }}",
                EqualTo,
                false
            },

            {
                $"{{{Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq' }}",
                EqualTo,
                false
            },

            {
                $"{{{Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'contains', {Filter.ValueJsonPropertyName} : 'br' }}",
                Contains,
                true
            },

            {
                $"{{{Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'contains', {Filter.ValueJsonPropertyName} : 6 }}",
                Contains,
                false
            },

            {
                $"{{{Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'isnull', {Filter.ValueJsonPropertyName} : 6 }}",
                IsNull,
                false
            }
        };

    [Theory]
    [MemberData(nameof(FilterSchemaTestCases))]
    public void FilterSchema(string json, FilterOperator @operator, bool expectedValidity)
    {
        output.WriteLine($"{nameof(json)} : {json}");
        output.WriteLine($"{nameof(FilterOperator)} : {@operator}");

        // Arrange
        JSchema schema = Filter.Schema(@operator);

        // Act
        bool isValid = JObject.Parse(json).IsValid(schema);

        // Arrange
        isValid.Should().Be(expectedValidity);
    }

    public static TheoryData<Filter, Filter, bool> FilterEquatableCases
    {
        get
        {
            TheoryData<Filter, Filter, bool> cases = new()
            {
                {
                    new Filter("property", EqualTo, "value"),
                    new Filter("property", EqualTo, "value"),
                    true
                },

                {
                    new Filter("property", IsNull),
                    new Filter("property", IsNull, null),
                    true
                },

                {
                    new Filter("property", EqualTo, null),
                    new Filter("property", IsNull),
                    true
                },

                {
                    new Filter("property", EqualTo, "value"),
                    new Filter("property", NotEqualTo, "value"),
                    false
                },

                {
                    new Filter("Property", EqualTo, "value"),
                    new Filter("property", EqualTo, "value"),
                    false
                },
            };

            Filter current = new("Property", EqualTo, "value");

            cases.Add(current, current, true);

            Guid guid = Guid.NewGuid();
            cases.Add(new Filter("Prop", EqualTo, guid), new Filter("Prop", EqualTo, guid),
                true);

            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(FilterEquatableCases))]
    public void FilterImplementsEquatableProperly(Filter first, Filter second, bool expectedResult)
    {
        output.WriteLine($"first : {first}");
        output.WriteLine($"second : {second}");

        // Act
        bool result = first.Equals(second);

        // Assert
        result.Should()
            .Be(expectedResult);
    }

    [Fact]
    public void Ctor_should_build_valid_instance()
    {
        // Act

        Regex validFieldNameRegex = new(Filter.ValidFieldNamePattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

        Prop.ForAll<string, FilterOperator, object>((field, @operator, value) =>
            {
                Lazy<Filter> filterCtor = new(() => new Filter(field, @operator, value));
                Filter filter = null;
                Action invokingCtor = () => filter = filterCtor.Value;
                // Assertion
                ((Action)(() => invokingCtor.Should().Throw<ArgumentOutOfRangeException>())).When(field is not null && !validFieldNameRegex.IsMatch(field))
                    .Label($"'{field}' doesnt not match expected regex ")
                    //.Trivial(field is not null && !validFieldNameRegex.IsMatch(field))
                    .Or(() =>
                    {
                        return filter.Field == field
                               && filter.Operator == @operator
                               && filter.Value is null;
                    }).When(Filter.UnaryOperators.Contains(@operator)).Label("Unary operator")
                    .Or(() =>
                    {
                        Filter filter = filterCtor.Value;

                        return filter.Field == field
                               && filter.Operator == @operator
                               && value.Equals(filter.Value);
                    }).When(!Filter.UnaryOperators.Contains(@operator)).Label("Binary operator")
                    .VerboseCheck(output);
            })
            .VerboseCheck(output);
    }

    [Property(Arbitrary = [typeof(FilterGenerators)])]
    public void Given_filter_instance_Negate_should_work_as_expected(NonNull<Filter> source)
    {
        // Arrange
        Filter original = source.Item;

        // Act
        IFilter oppositeFilter = original.Negate();

        // Assert
        oppositeFilter.Should()
            .BeOfType<Filter>()
            .Which
            .Operator.Should()
            .Be(original.Operator switch
            {
                EqualTo => NotEqualTo,
                NotEqualTo => EqualTo,
                IsNull => IsNotNull,
                IsNotNull => IsNull,
                FilterOperator.LessThan => GreaterThan,
                GreaterThan => FilterOperator.LessThan,
                GreaterThanOrEqual => LessThanOrEqualTo,
                StartsWith => NotStartsWith,
                NotStartsWith => StartsWith,
                EndsWith => NotEndsWith,
                NotEndsWith => EndsWith,
                Contains => NotContains,
                NotContains => Contains,
                IsEmpty => IsNotEmpty,
                IsNotEmpty => IsEmpty,
                LessThanOrEqualTo => GreaterThanOrEqual,
                _ => throw new NotSupportedException($"The original {original.Operator} operator is not supported"),
            }, "the resulting filter should have the opposite operator");
    }
}
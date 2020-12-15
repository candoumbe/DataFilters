using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static DataFilters.FilterLogic;
using static DataFilters.FilterOperator;
#if NETCOREAPP2_1
using static Newtonsoft.Json.JsonConvert;
#else
using static System.Text.Json.JsonSerializer;
#endif

namespace DataFilters.UnitTests
{
    [UnitTest]
    public class FilterTests
    {
        private readonly ITestOutputHelper _output;

        private static readonly IImmutableDictionary<string, FilterOperator> _operators = new Dictionary<string, FilterOperator>
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
        public static IEnumerable<object[]> FilterDeserializeCases
        {
            get
            {
                foreach (KeyValuePair<string, FilterOperator> item in _operators)
                {
                    yield return new object[]
                    {
                        @$"{{ ""field"" : ""Firstname"", ""op"" : ""{item.Key}"",  ""Value"" : ""Batman""}}",
                        (Expression<Func<IFilter, bool>>)(result => result is Filter
                            && "Firstname".Equals(((Filter) result).Field)
                            && item.Value.Equals(((Filter) result).Operator)
                            && "Batman".Equals(((Filter) result).Value)
                        )
                    };
                }
            }
        }

        public static IEnumerable<object[]> CompositeFilterToJsonCases
        {
            get
            {
                yield return new object[]
                {
                    new MultiFilter  {
                        Logic = Or,
                        Filters = new [] {
                            new Filter (field : "Nickname", @operator : EqualTo, value : "Batman"),
                            new Filter (field : "Nickname", @operator : EqualTo, value : "Robin")
                        }
                    },
                    (Expression<Func<string, bool>>)(json =>
                        JObject.Parse(json).Properties().Count() == 2

                        && "or".Equals((string) JObject.Parse(json)[MultiFilter.LogicJsonPropertyName])

                        && "Nickname".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][0][Filter.FieldJsonPropertyName])
                        && "eq".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][0][Filter.OperatorJsonPropertyName])
                        && "Batman".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][0][Filter.ValueJsonPropertyName])
                               &&
                        "Nickname".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][1][Filter.FieldJsonPropertyName])
                        && "eq".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][1][Filter.OperatorJsonPropertyName])
                        && "Robin".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][1][Filter.ValueJsonPropertyName])

                    )
                };

                yield return new object[]
                {
                    new MultiFilter  {
                        Filters = new [] {
                            new Filter (field : "Nickname", @operator : EqualTo, value : "Batman"),
                            new Filter (field : "Nickname", @operator : EqualTo, value : "Robin")
                        }
                    },
                    (Expression<Func<string, bool>>)(json =>
                        JObject.Parse(json).Properties().Count() == 2

                        && "and".Equals((string) JObject.Parse(json)[MultiFilter.LogicJsonPropertyName])

                        && "Nickname".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][0][Filter.FieldJsonPropertyName])
                        && "eq".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][0][Filter.OperatorJsonPropertyName])
                        && "Batman".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][0][Filter.ValueJsonPropertyName])
                               &&
                        "Nickname".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][1][Filter.FieldJsonPropertyName])
                        && "eq".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][1][Filter.OperatorJsonPropertyName])
                        && "Robin".Equals((string)JObject.Parse(json)[MultiFilter.FiltersJsonPropertyName][1][Filter.ValueJsonPropertyName])

                    )
                };
            }
        }

        public static IEnumerable<object[]> FilterSchemaTestCases
        {
            get
            {
                yield return new object[]
                {
                    $"{{{Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} :'batman' }}",
                    EqualTo,
                    true
                };

                yield return new object[]
                {
                    $"{{{Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : null }}",
                    EqualTo,
                    false
                };

                yield return new object[]
                {
                    $"{{{Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq' }}",
                    EqualTo,
                    false
                };

                yield return new object[]
                {
                    $"{{{Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'contains', {Filter.ValueJsonPropertyName} : 'br' }}",
                    Contains,
                    true
                };

                yield return new object[]
                {
                    $"{{{Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'contains', {Filter.ValueJsonPropertyName} : 6 }}",
                    Contains,
                    false
                };

                yield return new object[]
                {
                    $"{{{Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'isnull', {Filter.ValueJsonPropertyName} : 6 }}",
                    IsNull,
                    false
                };
            }
        }

        public FilterTests(ITestOutputHelper output) => _output = output;

        /// <summary>
        /// Serialization of instance of <see cref="Filter"/> test cases
        /// </summary>
        public static IEnumerable<object[]> FilterToJsonCases
        {
            get
            {
                yield return new object[]
                {
                    new Filter (field : "Firstname", @operator  : EqualTo,  value : "Batman"),
                    (Expression<Func<string, bool>>)(json =>
                        "Firstname".Equals((string) JObject.Parse(json)[Filter.FieldJsonPropertyName])
                        && "eq".Equals((string) JObject.Parse(json)[Filter.OperatorJsonPropertyName])
                        && "Batman".Equals((string) JObject.Parse(json)[Filter.ValueJsonPropertyName])
                    )
                };
            }
        }

        [Theory]
        [MemberData(nameof(FilterToJsonCases))]
        public void FilterToJson(Filter filter, Expression<Func<string, bool>> jsonMatcher)
            => ToJson(filter, jsonMatcher);

        private void ToJson(IFilter filter, Expression<Func<string, bool>> jsonMatcher)
        {
            _output.WriteLine($"Testing : {filter}{Environment.NewLine} against {Environment.NewLine} {jsonMatcher} ");

            // Act
            string json = filter.ToJson();

            // Assert
            _output.WriteLine($"ToJson result is '{json}'");
            json.Should().Match(jsonMatcher);
        }

        [Theory]
        [MemberData(nameof(FilterSchemaTestCases))]
        public void FilterSchema(string json, FilterOperator @operator, bool expectedValidity)
        {
            _output.WriteLine($"{nameof(json)} : {json}");
            _output.WriteLine($"{nameof(FilterOperator)} : {@operator}");

            // Arrange
            JSchema schema = Filter.Schema(@operator);

            // Act
            bool isValid = JObject.Parse(json).IsValid(schema);

            // Arrange
            isValid.Should().Be(expectedValidity);
        }

        public static IEnumerable<object[]> FilterEquatableCases
        {
            get
            {
                yield return new object[]
                {
                    new Filter("property", EqualTo, "value"),
                    new Filter("property", EqualTo, "value"),
                    true
                };

                yield return new object[]
                {
                    new Filter("property", IsNull ),
                    new Filter("property", IsNull, null),
                    true
                };

                yield return new object[]
                {
                    new Filter("property", EqualTo, null),
                    new Filter("property", IsNull),
                    true
                };

                yield return new object[]
                {
                    new Filter("property", EqualTo, "value"),
                    new Filter("property", NotEqualTo, "value"),
                    false
                };

                yield return new object[]
                {
                    new Filter("Property", EqualTo, "value"),
                    new Filter("property", EqualTo, "value"),
                    false
                };

                {
                    Filter first = new Filter("Property", EqualTo, "value");
                    yield return new object[]
                    {
                        first,
                        first,
                        true
                    };
                }

                {
                    Guid guid = Guid.NewGuid();
                    yield return new object[]
                    {
                        new Filter("Prop", EqualTo, guid),
                        new Filter("Prop", EqualTo, guid),
                        true
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(FilterEquatableCases))]
        public void FilterImplementsEquatableProperly(Filter first, Filter second, bool expectedResult)
        {
            _output.WriteLine($"first : {first}");
            _output.WriteLine($"second : {second}");

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

            Regex validFieldNameRegex = new Regex(Filter.ValidFieldNamePattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

            Prop.ForAll<string, FilterOperator, object>((field, @operator, value) =>
            {
                Lazy<Filter> filterCtor = new Lazy<Filter>(() => new Filter(field, @operator, value));

                // Assertion
                Prop.Throws<ArgumentOutOfRangeException, Filter>(filterCtor).When(field is not null && !validFieldNameRegex.IsMatch(field))
                                                                               .Label($"'{field}' doesnt not match expected regex ")
                    //.Trivial(field is not null && !validFieldNameRegex.IsMatch(field))
                    .Or(() =>
                    {
                        Filter filter = filterCtor.Value;

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
                    .VerboseCheck(_output);
            })
            .VerboseCheck(_output);
        }
    }
}

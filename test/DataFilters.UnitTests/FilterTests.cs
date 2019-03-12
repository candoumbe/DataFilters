using FluentAssertions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;
using static DataFilters.FilterLogic;
using static DataFilters.FilterOperator;
using static Newtonsoft.Json.JsonConvert;

namespace DataFilters.UnitTests
{
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
            ["lt"] = LessThan,
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

                        $"{{ field = 'Firstname', operator = '{item.Key}',  Value = 'Batman'}}",
                        ((Expression<Func<IFilter, bool>>)(result => result is Filter
                            && "Firstname".Equals(((Filter) result).Field)
                            && item.Value.Equals(((Filter) result).Operator) &&
                            "Batman".Equals(((Filter) result).Value)
                        ))
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
                    new CompositeFilter  {
                        Logic = Or,
                        Filters = new [] {
                            new Filter (field : "Nickname", @operator : EqualTo, value : "Batman"),
                            new Filter (field : "Nickname", @operator : EqualTo, value : "Robin")
                        }
                    },
                    ((Expression<Func<string, bool>>)(json =>
                        JObject.Parse(json).Properties().Count() == 2 &&

                        "or".Equals((string) JObject.Parse(json)[CompositeFilter.LogicJsonPropertyName]) &&

                        "Nickname".Equals((string)JObject.Parse(json)[CompositeFilter.FiltersJsonPropertyName][0][Filter.FieldJsonPropertyName]) &&
                        "eq".Equals((string)JObject.Parse(json)[CompositeFilter.FiltersJsonPropertyName][0][Filter.OperatorJsonPropertyName]) &&
                        "Batman".Equals((string)JObject.Parse(json)[CompositeFilter.FiltersJsonPropertyName][0][Filter.ValueJsonPropertyName])
                               &&
                        "Nickname".Equals((string)JObject.Parse(json)[CompositeFilter.FiltersJsonPropertyName][1][Filter.FieldJsonPropertyName]) &&
                        "eq".Equals((string)JObject.Parse(json)[CompositeFilter.FiltersJsonPropertyName][1][Filter.OperatorJsonPropertyName]) &&
                        "Robin".Equals((string)JObject.Parse(json)[CompositeFilter.FiltersJsonPropertyName][1][Filter.ValueJsonPropertyName])

                    ))
                };

                yield return new object[]
                {
                    new CompositeFilter  {
                        Filters = new [] {
                            new Filter (field : "Nickname", @operator : EqualTo, value : "Batman"),
                            new Filter (field : "Nickname", @operator : EqualTo, value : "Robin")

                        }
                    },
                    ((Expression<Func<string, bool>>)(json =>
                        JObject.Parse(json).Properties().Count() == 2 &&

                        "and".Equals((string) JObject.Parse(json)[CompositeFilter.LogicJsonPropertyName]) &&

                        "Nickname".Equals((string)JObject.Parse(json)[CompositeFilter.FiltersJsonPropertyName][0][Filter.FieldJsonPropertyName]) &&
                        "eq".Equals((string)JObject.Parse(json)[CompositeFilter.FiltersJsonPropertyName][0][Filter.OperatorJsonPropertyName]) &&
                        "Batman".Equals((string)JObject.Parse(json)[CompositeFilter.FiltersJsonPropertyName][0][Filter.ValueJsonPropertyName])
                               &&
                        "Nickname".Equals((string)JObject.Parse(json)[CompositeFilter.FiltersJsonPropertyName][1][Filter.FieldJsonPropertyName]) &&
                        "eq".Equals((string)JObject.Parse(json)[CompositeFilter.FiltersJsonPropertyName][1][Filter.OperatorJsonPropertyName]) &&
                        "Robin".Equals((string)JObject.Parse(json)[CompositeFilter.FiltersJsonPropertyName][1][Filter.ValueJsonPropertyName])

                    ))
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
                    ((Expression<Func<string, bool>>)(json =>
                        "Firstname".Equals((string) JObject.Parse(json)[Filter.FieldJsonPropertyName]) &&
                        "eq".Equals((string) JObject.Parse(json)[Filter.OperatorJsonPropertyName]) &&
                        "Batman".Equals((string) JObject.Parse(json)[Filter.ValueJsonPropertyName])
                    ))
                };
            }
        }

        [Theory]
        [MemberData(nameof(FilterToJsonCases))]
        public void FilterToJson(Filter filter, Expression<Func<string, bool>> jsonMatcher)
            => ToJson(filter, jsonMatcher);


        public static IEnumerable<object[]> CompositeFilterSchemaTestCases
        {
            get
            {
                yield return new object[]
                {
                    "{" +
                        $"{CompositeFilter.LogicJsonPropertyName} : 'or'," +
                        $"{CompositeFilter.FiltersJsonPropertyName}: [" +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'batman' }}," +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'robin' }}" +
                        "]" +
                    "}",
                    true
                };

                yield return new object[]
                {
                    "{" +
                        $"{CompositeFilter.LogicJsonPropertyName} : 'and'," +
                        $"{CompositeFilter.FiltersJsonPropertyName}: [" +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'batman' }}," +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'robin' }}" +
                        "]" +
                    "}",
                    true
                };

                yield return new object[]
                {
                    "{" +
                        $"{CompositeFilter.FiltersJsonPropertyName}: [" +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'batman' }}," +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'robin' }}" +
                        "]" +
                    "}",
                    true
                };

                yield return new object[]
                {
                    "{" +
                        $"{CompositeFilter.FiltersJsonPropertyName}: [" +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'robin' }}" +
                        "]" +
                    "}",
                    false
                };

            }
        }

        [Theory]
        [MemberData(nameof(CompositeFilterToJsonCases))]
        public void CompositeFilterToJson(CompositeFilter filter, Expression<Func<string, bool>> jsonMatcher)
            => ToJson(filter, jsonMatcher);


        public static IEnumerable<object[]> CollectionOfFiltersCases
        {
            get
            {
                yield return new object[] {
                    new IFilter[]
                    {
                        new Filter (field : "Firstname", @operator : EqualTo, value : "Bruce"),
                        new Filter (field : "Lastname", @operator : EqualTo, value : "Wayne" )
                    },
                    ((Expression<Func<string, bool>>)(json =>
                        JToken.Parse(json).Type == JTokenType.Array
                        && JArray.Parse(json).Count == 2


                        && JArray.Parse(json)[0].Type == JTokenType.Object
                        && JArray.Parse(json)[0].IsValid(Filter.Schema(EqualTo))
                        && JArray.Parse(json)[0][Filter.FieldJsonPropertyName].Value<string>() == "Firstname"
                        && JArray.Parse(json)[0][Filter.OperatorJsonPropertyName].Value<string>() == "eq"
                        && JArray.Parse(json)[0][Filter.ValueJsonPropertyName].Value<string>() == "Bruce"

                        && JArray.Parse(json)[1].Type == JTokenType.Object
                        && JArray.Parse(json)[1].IsValid(Filter.Schema(EqualTo))
                        && JArray.Parse(json)[1][Filter.FieldJsonPropertyName].Value<string>() == "Lastname"
                        && JArray.Parse(json)[1][Filter.OperatorJsonPropertyName].Value<string>() == "eq"
                        && JArray.Parse(json)[1][Filter.ValueJsonPropertyName].Value<string>() == "Wayne"
                    ))

                };
            }
        }

        [Theory]
        [MemberData(nameof(CollectionOfFiltersCases))]
        public void CollectionOfFiltersToJson(IEnumerable<IFilter> filters, Expression<Func<string, bool>> jsonExpectation)
        {
            // Act
            string json = SerializeObject(filters);

            _output.WriteLine($"result of the serialization : {json}");

            // Assert
            json.Should().Match(jsonExpectation);
        }

        private void ToJson(IFilter filter, Expression<Func<string, bool>> jsonMatcher)
        {
            _output.WriteLine($"Testing : {filter}{Environment.NewLine} against {Environment.NewLine} {jsonMatcher} ");
            filter.ToJson().Should().Match(jsonMatcher);
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


        [Theory]
        [MemberData(nameof(CompositeFilterSchemaTestCases))]
        public void CompositeFilterSchema(string json, bool expectedValidity)
        {
            _output.WriteLine($"{nameof(json)} : {json}");

            // Arrange
            JSchema schema = CompositeFilter.Schema;

            // Act
            bool isValid = JObject.Parse(json).IsValid(schema);

            // Assert
            isValid.Should().Be(expectedValidity);
        }
    }
}

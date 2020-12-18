using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;
using static DataFilters.FilterOperator;
using static DataFilters.FilterLogic;
using Xunit.Categories;
using Newtonsoft.Json.Schema;
using DataFilters.Converters;
#if NETCOREAPP2_1
using static Newtonsoft.Json.JsonConvert;
#endif
namespace DataFilters.UnitTests.Converters
{
    [UnitTest]
    [Feature("Converters")]
    public class DataFilterConverterTests
    {
        private readonly ITestOutputHelper _outputHelper;

        private static IImmutableDictionary<string, FilterOperator> Operators => new Dictionary<string, FilterOperator>
        {
            ["eq"] = EqualTo,
            ["neq"] = NotEqualTo,
            ["lt"] = FilterOperator.LessThan,
            ["gt"] = GreaterThan,
            ["lte"] = LessThanOrEqualTo,
            ["gte"] = GreaterThanOrEqual,
            ["contains"] = Contains,
            ["isnull"] = IsNull,
            ["isnotnull"] = IsNotNull,
            ["isnotempty"] = IsNotEmpty,
            ["isempty"] = IsEmpty
        }.ToImmutableDictionary();

        public DataFilterConverterTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        /// <summary>
        /// Deserialize tests cases
        /// </summary>
        public static IEnumerable<object[]> SerializeCases
        {
            get
            {
                foreach (KeyValuePair<string, FilterOperator> item in Operators.Where(kv => !Filter.UnaryOperators.Any(op => op == kv.Value)))
                {
                    yield return new object[]
                    {
                        new Filter(field : "Firstname", @operator : item.Value, value : "Bruce"),
                        (Expression<Func<string, bool>>) ((json) => json != null
                            && JToken.Parse(json).Type == JTokenType.Object
                            && JToken.Parse(json).IsValid(Filter.Schema(item.Value))
                            && "Firstname".Equals(JToken.Parse(json)[Filter.FieldJsonPropertyName].Value<string>())
                            && item.Key.Equals(JToken.Parse(json)[Filter.OperatorJsonPropertyName].Value<string>())
                            && "Bruce".Equals(JToken.Parse(json)[Filter.ValueJsonPropertyName].Value<string>()))
                    };
                }

                foreach (KeyValuePair<string, FilterOperator> item in Operators.Where(kv => Filter.UnaryOperators.Any(op => op == kv.Value)))
                {
                    yield return new object[]{
                        new Filter(field : "Firstname", @operator : item.Value),
                        (Expression<Func<string, bool>>) ((json) => json != null
                            && JToken.Parse(json).Type == JTokenType.Object
                            && JObject.Parse(json).Properties().Exactly(2)
                            && "Firstname".Equals(JObject.Parse(json)[Filter.FieldJsonPropertyName].Value<string>())
                            && item.Key.Equals(JObject.Parse(json)[Filter.OperatorJsonPropertyName].Value<string>()))
                    };
                }
            }
        }

        /// <summary>
        /// Deserialize tests cases
        /// </summary>
        public static IEnumerable<object[]> DeserializeCases
        {
            get
            {
                foreach ((string key, FilterOperator value) in Operators.Where(op => op.Value != IsNull
                                                                                     && op.Value != IsNotNull
                                                                                     && op.Value != IsEmpty
                                                                                     && op.Value != IsNotEmpty))
                {
                    yield return new object[]
                    {
                        $@"{{""field"" :""Firstname"", ""op"":""{key}"", ""value"" : ""Bruce""}}",
                        typeof(Filter),
                        (Expression<Func<object, bool>>) ((result) => new Filter("Firstname", value, "Bruce").Equals(result))
                    };
                }

                yield return new object[]
                {
                    @"{""field"" :""Firstname"", ""op"":""isnull""}",
                    typeof(Filter),
                    (Expression<Func<object, bool>>) ((result) => new Filter("Firstname", IsNull, null).Equals(result))
                };

                yield return new object[]
                {
                       @"{""field"" :""Firstname"", ""op"":""isnull"", ""value"" : 6}",
                       typeof(Filter),
                       (Expression<Func<object, bool>>) ((result) => new Filter("Firstname", IsNull, null).Equals(result))
                };

                yield return new object[]
                {
                       @"{""field"" :""Firstname"", ""op"":""isnotnull"", ""value"" : 6}",
                       typeof(Filter),
                       (Expression<Func<object, bool>>) ((result) => new Filter("Firstname", IsNotNull, null).Equals(result))
                };

                yield return new object[]
                {
                    @$"{{""field"" :""integer"", ""op"":""eq"", ""value"" : {long.MaxValue}}}",
                    typeof(Filter),
                    (Expression<Func<object, bool>>) ((result) => new Filter("integer", EqualTo, long.MaxValue).Equals(result))
                };

                yield return new object[]
                {
                    @"{""field"" :""bool"", ""op"":""eq"", ""value"" : true }",
                    typeof(Filter),
                    (Expression<Func<object, bool>>) ((result) => new Filter("bool", EqualTo, true ).Equals(result))
                };

                yield return new object[]
                {
                    @"{""field"" :""EqualNull"", ""op"":""eq"", ""value"" : null }",
                    typeof(Filter),
                    (Expression<Func<object, bool>>) ((result) => new Filter("EqualNull", IsNull, null).Equals(result))
                };

                yield return new object[]
                {
                    @"{""field"" :""NotEqualNull"", ""op"":""neq"", ""value"" : null }",
                    typeof(Filter),
                    (Expression<Func<object, bool>>) ((result) => new Filter("NotEqualNull", IsNotNull, null).Equals(result))
                };

                {
                    Guid uuid = Guid.NewGuid();
                    yield return new object[]
                    {
                        $@"{{""field"" :""guid"", ""op"":""eq"", ""value"" : ""{uuid}"" }}",
                        typeof(Filter),
                        (Expression<Func<object, bool>>) ((result) => new Filter("guid", EqualTo, uuid.ToString() ).Equals(result))
                    };
                }

                {
                    yield return new object[]
                    {
                        @"{""field"" :""date"", ""op"":""eq"", ""value"" : ""2019-8-23T00:00"" }",
                        typeof(Filter),
                        (Expression<Func<object, bool>>) ((result) => new Filter("date", EqualTo, "2019-8-23T00:00" ).Equals(result))
                    };
                }

                yield return new object[]
                {
                    "{" +
                        @"""logic"": ""or""," +
                        @"""filters"": [" +
                            @"{ ""field"" :""Firstname"", ""op"":""eq"", ""value"":""Bruce""}," +
                            @"{ ""field"" :""Firstname"", ""op"":""eq"", ""value"":""Clark""}" +
                        "]" +
                    "}",
                    typeof(MultiFilter),
                    (Expression<Func<object, bool>>) ((result) =>
                        new MultiFilter
                        {
                            Logic = Or,
                            Filters = new IFilter[]
                            {
                                new Filter("Firstname", EqualTo, "Bruce"),
                                new Filter("Firstname", EqualTo, "Clark")
                            }
                        }.Equals(result)
                    )
                };

                yield return new object[]
                {
                    "{" +
                        @"""logic"": ""and""," +
                        @"""filters"": [" +
                            @"{ ""field"" :""HasSuperpower"", ""op"":""eq"", ""value"": false}," +
                            "{" +
                                @"""logic"": ""or""," +
                                @"""filters"": [" +
                                    @"{ ""field"" :""Lastname"", ""op"":""eq"", ""value"":""Wayne""}," +
                                    @"{ ""field"" :""Lastname"", ""op"":""eq"", ""value"":""Kent""}" +
                                "]" +
                            "}" +
                        "]" +
                    "}",
                    typeof(MultiFilter),
                    (Expression<Func<object, bool>>) ((result) =>
                        new MultiFilter
                        {
                            Logic = And,
                            Filters = new IFilter[]
                            {
                                new Filter("HasSuperpower", EqualTo, false),
                                new MultiFilter
                                {
                                    Logic = Or,
                                    Filters = new IFilter[]
                                    {
                                        new Filter("Lastname", EqualTo, "Wayne"),
                                        new Filter("Lastname", EqualTo, "Kent")
                                    }
                                }
                            }
                        }.Equals(result)
                    )
                };
            }
        }

        /// <summary>
        /// Tests the deserialization of the <paramref name="json"/> to an instance of the specified <paramref name="targetType"/> <br/>
        /// The deserialization is done using <c>JsonConvert.DeserializeObject</c>
        /// </summary>
        /// <param name="json">json to deserialize</param>
        /// <param name="targetType">type the json string will be deserialize into</param>
        /// <param name="expectation">Expectation that result of the deserialization should match</param>
        [Theory]
        [MemberData(nameof(DeserializeCases))]
        public void Deserialize(string json, Type targetType, Expression<Func<object, bool>> expectation)
        {
            _outputHelper.WriteLine($"{nameof(json)} : {json}");

            object result = System.Text.Json.JsonSerializer.Deserialize(json, targetType);

            result.Should()
                  .Match(expectation);
        }

        /// <summary>
        /// Tests the serialization of the <paramref name="obj"/> to its string representation
        /// The deserialization is done using <c>JsonConvert.DeserializeObject</c>
        /// </summary>
        /// <param name="filter">json to deserialize</param>
        /// <param name="expectation">Expectation that result of the deserialization should match</param>
        [Theory]
        [MemberData(nameof(SerializeCases))]
        public void Serialize(IFilter filter, Expression<Func<string, bool>> expectation)
        {
            _outputHelper.WriteLine($"Serializing {filter}");

            string result = System.Text.Json.JsonSerializer.Serialize(filter, filter.GetType());

            result.Should()
                .Match(expectation);
        }
    }
}

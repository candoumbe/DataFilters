#if NETCOREAPP2_1
using static Newtonsoft.Json.JsonConvert;
#endif
namespace DataFilters.UnitTests.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;
    using static DataFilters.FilterLogic;
    using static DataFilters.FilterOperator;

    [UnitTest]
    [Feature("Converters")]
    public class DataFilterConverterTests(ITestOutputHelper outputHelper)
    {
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

        /// <summary>
        /// Deserialize tests cases
        /// </summary>
        public static TheoryData<Filter, Expression<Func<string, bool>>> SerializeCases
        {
            get
            {
                TheoryData<Filter, Expression<Func<string, bool>>> cases = [];
                foreach (KeyValuePair<string, FilterOperator> item in Operators.Where(kv => !Filter.UnaryOperators.Any(op => op == kv.Value)))
                {
                    cases.Add
                    (
                        new Filter(field: "Firstname", @operator: item.Value, value: "Bruce"),
                         (json) => json != null
                            && JToken.Parse(json).Type == JTokenType.Object
                            && JToken.Parse(json).IsValid(Filter.Schema(item.Value))
                            && "Firstname".Equals(JToken.Parse(json)[Filter.FieldJsonPropertyName].Value<string>())
                            && item.Key.Equals(JToken.Parse(json)[Filter.OperatorJsonPropertyName].Value<string>())
                            && "Bruce".Equals(JToken.Parse(json)[Filter.ValueJsonPropertyName].Value<string>())
                    );
                }

                foreach (KeyValuePair<string, FilterOperator> item in Operators.Where(kv => Filter.UnaryOperators.Any(op => op == kv.Value)))
                {
                    cases.Add
                    (
                        new Filter(field: "Firstname", @operator: item.Value),
                         (json) => json != null
                            && JToken.Parse(json).Type == JTokenType.Object
                            && JObject.Parse(json).Properties().Exactly(2)
                            && "Firstname".Equals(JObject.Parse(json)[Filter.FieldJsonPropertyName].Value<string>())
                            && item.Key.Equals(JObject.Parse(json)[Filter.OperatorJsonPropertyName].Value<string>())
                    );
                }

                return cases;
            }
        }

        /// <summary>
        /// Deserialize tests cases
        /// </summary>
        public static TheoryData<string, Type, Expression<Func<object, bool>>> DeserializeCases
        {
            get
            {
                TheoryData<string, Type, Expression<Func<object, bool>>> cases = [];
                foreach ((string key, FilterOperator value) in Operators.Where(op => op.Value != IsNull
                                                                                     && op.Value != IsNotNull
                                                                                     && op.Value != IsEmpty
                                                                                     && op.Value != IsNotEmpty))
                {
                    cases.Add
                    (
                        $@"{{""field"" :""Firstname"", ""op"":""{key}"", ""value"" : ""Bruce""}}",
                        typeof(Filter),
                        result => new Filter("Firstname", value, "Bruce").Equals(result)
                    );
                }

                cases.Add
                (
                    @"{""field"" :""Firstname"", ""op"":""isnull""}",
                    typeof(Filter),
                     (result) => new Filter("Firstname", IsNull, null).Equals(result)
                );

                cases.Add
                (
                       @"{""field"" :""Firstname"", ""op"":""isnull"", ""value"" : 6}",
                       typeof(Filter),
                        (result) => new Filter("Firstname", IsNull, null).Equals(result)
                );

                cases.Add
                (
                       @"{""field"" :""Firstname"", ""op"":""isnotnull"", ""value"" : 6}",
                       typeof(Filter),
                        (result) => new Filter("Firstname", IsNotNull, null).Equals(result)
                );

                cases.Add
                (
                    @$"{{""field"" :""integer"", ""op"":""eq"", ""value"" : {long.MaxValue}}}",
                    typeof(Filter),
                     (result) => new Filter("integer", EqualTo, long.MaxValue).Equals(result)
                );

                cases.Add
                (
                    @"{""field"" :""bool"", ""op"":""eq"", ""value"" : true }",
                    typeof(Filter),
                     (result) => new Filter("bool", EqualTo, true).Equals(result)
                );

                cases.Add
                (
                    @"{""field"" :""EqualNull"", ""op"":""eq"", ""value"" : null }",
                    typeof(Filter),
                     (result) => new Filter("EqualNull", IsNull, null).Equals(result)
                );

                cases.Add
                (
                    @"{""field"" :""NotEqualNull"", ""op"":""neq"", ""value"" : null }",
                    typeof(Filter),
                     (result) => new Filter("NotEqualNull", IsNotNull, null).Equals(result)
                );

                {
                    Guid uuid = Guid.NewGuid();
                    cases.Add
                    (
                        $@"{{""field"" :""guid"", ""op"":""eq"", ""value"" : ""{uuid}"" }}",
                        typeof(Filter),
                         (result) => new Filter("guid", EqualTo, uuid.ToString()).Equals(result)
                    );
                }

                cases.Add
                (
                    @"{""field"" :""date"", ""op"":""eq"", ""value"" : ""2019-8-23T00:00"" }",
                    typeof(Filter),
                     (result) => new Filter("date", EqualTo, "2019-8-23T00:00").Equals(result)
                );

                cases.Add
                (
                    "{" +
                        @"""logic"": ""or""," +
                        @"""filters"": [" +
                            @"{ ""field"" :""Firstname"", ""op"":""eq"", ""value"":""Bruce""}," +
                            @"{ ""field"" :""Firstname"", ""op"":""eq"", ""value"":""Clark""}" +
                        "]" +
                    "}",
                    typeof(MultiFilter),
                     (result) =>
                        new MultiFilter
                        {
                            Logic = Or,
                            Filters = new IFilter[]
                            {
                                new Filter("Firstname", EqualTo, "Bruce"),
                                new Filter("Firstname", EqualTo, "Clark")
                            }
                        }.Equals(result)

                );

                cases.Add
                (
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
                     (result) =>
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

                );

                return cases;
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
            outputHelper.WriteLine($"{nameof(json)} : {json}");

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
            outputHelper.WriteLine($"Serializing {filter}");

            string result = System.Text.Json.JsonSerializer.Serialize(filter, filter.GetType());

            result.Should()
                .Match(expectation);
        }
    }
}

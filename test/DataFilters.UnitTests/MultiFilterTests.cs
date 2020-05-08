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
using Xunit.Categories;
using static DataFilters.FilterLogic;
using static DataFilters.FilterOperator;
using static Newtonsoft.Json.JsonConvert;

namespace DataFilters.UnitTests
{
    [UnitTest]
    public class MultiFilterTests
    {
        public MultiFilterTests(ITestOutputHelper output) => _output = output;

        private readonly ITestOutputHelper _output;

        public class Person
        {
            public string Firstname { get; set; }

            public string Lastname { get; set; }

            public DateTime BirthDate { get; set; }
        }

        public static IEnumerable<object[]> MultiFilterToJsonCases
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

        public static IEnumerable<object[]> CompositeFilterSchemaTestCases
        {
            get
            {
                yield return new object[]
                {
                    "{" +
                        $"{MultiFilter.LogicJsonPropertyName} : 'or'," +
                        $"{MultiFilter.FiltersJsonPropertyName}: [" +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'batman' }}," +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'robin' }}" +
                        "]" +
                    "}",
                    true
                };

                yield return new object[]
                {
                    "{" +
                        $"{MultiFilter.LogicJsonPropertyName} : 'and'," +
                        $"{MultiFilter.FiltersJsonPropertyName}: [" +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'batman' }}," +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'robin' }}" +
                        "]" +
                    "}",
                    true
                };

                yield return new object[]
                {
                    "{" +
                        $"{MultiFilter.FiltersJsonPropertyName}: [" +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'batman' }}," +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'robin' }}" +
                        "]" +
                    "}",
                    true
                };

                yield return new object[]
                {
                    "{" +
                        $"{MultiFilter.FiltersJsonPropertyName}: [" +
                            $"{{ {Filter.FieldJsonPropertyName} : 'nickname', {Filter.OperatorJsonPropertyName} : 'eq', {Filter.ValueJsonPropertyName} : 'robin' }}" +
                        "]" +
                    "}",
                    false
                };
            }
        }

        [Theory]
        [MemberData(nameof(MultiFilterToJsonCases))]
        public void MultiFilterToJson(MultiFilter filter, Expression<Func<string, bool>> jsonMatcher)
        {
            _output.WriteLine($"Testing : {filter}{Environment.NewLine} against {Environment.NewLine} {jsonMatcher} ");

            // Act
            string json = filter.ToJson();

            // Assert
            json.Should().Match(jsonMatcher);
        }

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
                    (Expression<Func<string, bool>>)(json =>
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
                    )
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

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]{
                            new Filter("property", EqualTo, "value"),
                            new Filter("property", EqualTo, "value"),
                        }
                    },
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]{
                            new Filter("property", EqualTo, "value"),
                            new Filter("property", EqualTo, "value"),
                        }
                    },
                    true,
                    $"Two instances of {nameof(MultiFilter)} contains same ${nameof(MultiFilter.Filters)} in same order"
                };

                yield return new object[]
                {
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]{
                            new Filter("property", EqualTo, "value"),
                            new Filter("property", EqualTo, "value"),
                        }
                    },
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]{
                            new Filter("property", NotEqualTo, "value"),
                            new Filter("property", EqualTo, "value"),
                        }
                    },
                    false,
                    "the second instance contains one filter that has a different operator"
                };

                yield return new object[]
                {
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]{
                            new Filter("property", EqualTo, "value"),
                            new Filter("property", EqualTo, "value"),
                        }
                    },
                    null,
                    false,
                    "comparing to null"
                };

                {
                    MultiFilter filter = new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]{
                            new Filter("property", EqualTo, "value"),
                            new Filter("property", EqualTo, "value"),
                        }
                    };
                    yield return new object[]
                    {
                        filter,
                        filter,
                        true,
                        "comparing a too itself must always returns true"
                    };
                }

                {
                    yield return new object[]
                    {
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
                        },
                        new MultiFilter
                        {
                            Logic = And,
                            Filters = new IFilter[]
                            {
                                new Filter("HasSuperpower", EqualTo, false),
                                new MultiFilter
                                {
                                    Logic = Or,
                                    Filters = new []
                                    {
                                        new Filter("Lastname", EqualTo, "Wayne"),
                                        new Filter("Lastname", EqualTo, "Kent")
                                    }
                                }
                            }
                        },
                        true,
                        "Two distinct instances of multifilters that holds same data in same order"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void CompositeFilterImplementsEquatableProperly(MultiFilter first, object second, bool expectedResult, string reason)
        {
            _output.WriteLine($"first : {first}");
            _output.WriteLine($"second : {second}");

            // Act
            bool result = first.Equals(second);

            // Assert
            result.Should()
                .Be(expectedResult, reason);
        }

        [Theory]
        [MemberData(nameof(CompositeFilterSchemaTestCases))]
        public void CompositeFilterSchema(string json, bool expectedValidity)
        {
            _output.WriteLine($"{nameof(json)} : {json}");

            // Arrange
            JSchema schema = MultiFilter.Schema;

            // Act
            bool isValid = JObject.Parse(json).IsValid(schema);

            // Assert
            isValid.Should().Be(expectedValidity);
        }
    }
}

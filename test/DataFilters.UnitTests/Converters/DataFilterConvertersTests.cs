using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static DataFilters.FilterOperator;

namespace DataFilters.UnitTests.Converters
{
    public class DataFilterConverterTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        private static IImmutableDictionary<string, FilterOperator> Operators => new Dictionary<string, FilterOperator>
        {
            ["eq"] = EqualTo,
            ["neq"] = NotEqualTo,
            ["lt"] = LessThan,
            ["gt"] = GreaterThan,
            ["lte"] = LessThanOrEqualTo,
            ["gte"] = GreaterThanOrEqual,
            ["contains"] = Contains,
            ["isnull"] = IsNull,
            ["isnotnull"] = IsNotNull,
            ["isnotempty"] = IsNotEmpty,
            ["isempty"] = IsEmpty
        }.ToImmutableDictionary();

        public DataFilterConverterTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose()
        {
            _outputHelper = null;
        }



        /// <summary>
        /// Deserialize tests cases
        /// </summary>
        public static IEnumerable<object[]> DeserializeCases
        {
            get
            {
                foreach (KeyValuePair<string, FilterOperator> item in Operators.Where(op => op.Value != IsNull && op.Value != IsNotNull && op.Value != IsEmpty && op.Value != IsNotEmpty))
                {
                    yield return new object[]{
                        $"{{{Filter.FieldJsonPropertyName} :'Firstname', {Filter.OperatorJsonPropertyName} :'{item.Key}', {Filter.ValueJsonPropertyName} : 'Bruce'}}",
                        typeof(Filter),
                        ((Expression<Func<object, bool>>) ((result) => result is Filter
                            && "Firstname" == ((Filter)result).Field
                            && item.Value == ((Filter)result).Operator
                            && ((Filter)result).Value is string
                            && "Bruce".Equals((string)((Filter)result).Value)))
                    };
                }

                yield return new object[]{
                        $"{{{Filter.FieldJsonPropertyName} :'Firstname', {Filter.OperatorJsonPropertyName} :'isnull'}}",
                        typeof(Filter),
                        ((Expression<Func<object, bool>>) ((result) => result is Filter
                            && "Firstname" == ((Filter)result).Field
                            && Operators["isnull"] == ((Filter)result).Operator
                            && ((Filter)result).Value == null))
                    };


                yield return new object[]{
                        $"{{{Filter.FieldJsonPropertyName} :'Firstname', {Filter.OperatorJsonPropertyName} :'isnull', {Filter.ValueJsonPropertyName} : 6}}",
                        typeof(Filter),
                        ((Expression<Func<object, bool>>) ((result) => result is Filter
                            && "Firstname" == ((Filter)result).Field
                            && Operators["isnull"] == ((Filter)result).Operator
                            && ((Filter)result).Value == null))
                    };

                yield return new object[]{
                        $"{{{Filter.FieldJsonPropertyName}:'Firstname', {Filter.OperatorJsonPropertyName} :'isnull', value : null}}",
                        typeof(Filter),
                        ((Expression<Func<object, bool>>) ((result) => result is Filter
                            && "Firstname" == ((Filter)result).Field
                            && Operators["isnull"] == ((Filter)result).Operator
                            && ((Filter)result).Value == null))
                    };

                yield return new object[]{
                        $"{{{Filter.FieldJsonPropertyName}:'Firstname', {Filter.OperatorJsonPropertyName} :'isnull', value : null}}",
                        typeof(Filter),
                        ((Expression<Func<object, bool>>) ((result) => result is Filter
                            && "Firstname" == ((Filter)result).Field
                            && Operators["isnull"] == ((Filter)result).Operator
                            && ((Filter)result).Value == null))
                    };

                yield return new object[]{
                        $"{{{Filter.FieldJsonPropertyName} :'Firstname', {Filter.OperatorJsonPropertyName} :'isnotnull', value : null}}",
                        typeof(Filter),
                        ((Expression<Func<object, bool>>) ((result) => result is Filter
                            && "Firstname" == ((Filter)result).Field
                            && Operators["isnotnull"] == ((Filter)result).Operator
                            && ((Filter)result).Value == null))
                    };

                yield return new object[]{
                        $"{{{Filter.FieldJsonPropertyName} :'Firstname', {Filter.OperatorJsonPropertyName} :'isnotnull'}}",
                        typeof(Filter),
                        ((Expression<Func<object, bool>>) ((result) => result is Filter
                            && "Firstname" == ((Filter)result).Field
                            && Operators["isnotnull"] == ((Filter)result).Operator
                            && ((Filter)result).Value == null))
                    };



            }
        }

        /// <summary>
        /// Deserialize tests cases
        /// </summary>
        public static IEnumerable<object[]> SerializeCases
        {
            get
            {

                foreach (KeyValuePair<string, FilterOperator> item in Operators.Where(kv => !Filter.UnaryOperators.Any(op => op == kv.Value)))
                {
                    yield return new object[]{
                        new Filter(field : "Firstname", @operator : item.Value, value : "Bruce"),
                        ((Expression<Func<string, bool>>) ((json) => json != null
                            && JToken.Parse(json).Type == JTokenType.Object
                            && JObject.Parse(json).Properties().Count() == 3
                            && "Firstname".Equals(JObject.Parse(json)[Filter.FieldJsonPropertyName].Value<string>())
                            && item.Key.Equals(JObject.Parse(json)[Filter.OperatorJsonPropertyName].Value<string>())
                            && "Bruce".Equals(JObject.Parse(json)[Filter.ValueJsonPropertyName].Value<string>())))


                    };
                }

                foreach (KeyValuePair<string, FilterOperator> item in Operators.Where(kv => Filter.UnaryOperators.Any(op => op == kv.Value)))
                {
                    yield return new object[]{
                        new Filter(field : "Firstname", @operator : item.Value),
                        ((Expression<Func<string, bool>>) ((json) => json != null
                            && JToken.Parse(json).Type == JTokenType.Object
                            && JObject.Parse(json).Properties().Count() == 2
                            && "Firstname".Equals(JObject.Parse(json)[Filter.FieldJsonPropertyName].Value<string>())
                            && item.Key.Equals(JObject.Parse(json)[Filter.OperatorJsonPropertyName].Value<string>())))

                    };
                }

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

            object result = JsonConvert.DeserializeObject(json, targetType);

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

            string result = JsonConvert.SerializeObject(filter);

            result.Should()
                .Match(expectation);
        }





        

    }
}

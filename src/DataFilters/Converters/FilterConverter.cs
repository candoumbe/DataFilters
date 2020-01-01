using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DataFilters.Converters
{
    /// <summary>
    /// <see cref="JsonConvert"/> implementation that can convert from/to <see cref="DataFilter"/>
    /// </summary>
    public class FilterConverter : JsonConverter
    {
        private readonly static IImmutableDictionary<string, FilterOperator> _operators = new Dictionary<string, FilterOperator>
        {
            ["contains"] = FilterOperator.Contains,
            ["ncontains"] = FilterOperator.NotContains,
            ["endswith"] = FilterOperator.EndsWith,
            ["nendswith"] = FilterOperator.NotEndsWith,
            ["eq"] = FilterOperator.EqualTo,
            ["neq"] = FilterOperator.NotEqualTo,
            ["gt"] = FilterOperator.GreaterThan,
            ["gte"] = FilterOperator.GreaterThanOrEqual,
            ["isempty"] = FilterOperator.IsEmpty,
            ["isnotempty"] = FilterOperator.IsNotEmpty,
            ["isnull"] = FilterOperator.IsNull,
            ["isnotnull"] = FilterOperator.IsNotNull,
            ["lt"] = FilterOperator.LessThan,
            ["lte"] = FilterOperator.LessThanOrEqualTo,
            ["startswith"] = FilterOperator.StartsWith,
            ["nstartswith"] = FilterOperator.NotStartsWith
        }.ToImmutableDictionary();

        public override bool CanConvert(Type objectType) => objectType == typeof(Filter);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Filter filter = null;

            JToken token = JToken.ReadFrom(reader);
            if (objectType == typeof(Filter))
            {
                if (token.Type == JTokenType.Object)
                {
                    IEnumerable<JProperty> properties = ((JObject)token).Properties();

                    if (properties.Any(prop => prop.Name == Filter.FieldJsonPropertyName)
                         && properties.Any(prop => prop.Name == Filter.OperatorJsonPropertyName))
                    {
                        string field = token[Filter.FieldJsonPropertyName].Value<string>();
                        FilterOperator @operator = _operators[token[Filter.OperatorJsonPropertyName].Value<string>()];
                        object value = null;
                        if (!Filter.UnaryOperators.Contains(@operator))
                        {
                            value = token[Filter.ValueJsonPropertyName]?.Value<string>();
                        }
                        filter = new Filter(field, @operator, value);
                    }
                }
            }

            return filter?.As(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Filter kf = (Filter)value;

            writer.WriteStartObject();

            // Field
            writer.WritePropertyName(Filter.FieldJsonPropertyName);
            writer.WriteValue(kf.Field);

            // operator
            writer.WritePropertyName(Filter.OperatorJsonPropertyName);
            KeyValuePair<string, FilterOperator> kv = _operators.Single(item => item.Value == kf.Operator);

            writer.WriteValue(kv.Key);

            // value (only if the operator is not an unary operator)
            if (!Filter.UnaryOperators.Contains(kf.Operator))
            {
                writer.WritePropertyName(Filter.ValueJsonPropertyName);
                writer.WriteValue(kf.Value);
            }

            writer.WriteEnd();
        }
    }
}

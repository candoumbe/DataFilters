namespace DataFilters.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

#if NETSTANDARD1_3
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
#else
    using System.Text.Json;
    using System.Text.Json.Serialization;
#endif

    /// <summary>
    /// <see cref="JsonConverter"/> implementation that can convert from/to <see cref="Filter"/>
    /// </summary>
#if NETSTANDARD1_3
    public class FilterConverter : JsonConverter
#else
    public class FilterConverter : JsonConverter<Filter>

#endif
{
    private readonly static IImmutableDictionary<string, FilterOperator> Operators = new Dictionary<string, FilterOperator>
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

#if NETSTANDARD1_3
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType) => objectType == typeof(Filter);

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Filter filter = null;

            JToken token = JToken.ReadFrom(reader);
            if (objectType == typeof(Filter) && token.Type == JTokenType.Object)
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
                        JToken valueToken = token[Filter.ValueJsonPropertyName];
                        value = valueToken?.Type switch
                        {
                            JTokenType.String => valueToken.Value<string>(),
                            JTokenType.Boolean => valueToken.Value<bool>(),
                            JTokenType.Integer => valueToken.Value<long>(),
                            JTokenType.Float => valueToken.Value<float>(),
                            JTokenType.Null => null,
                            null => null,
                            _ => throw new NotSupportedException($"Unexpected valueTokenType {valueToken.Type} when dealing with operator {@operator}")

                        };
                    }
                    filter = new Filter(field, @operator, value);
                }
            }

            return filter?.As(objectType);
        }

#else
        ///<inheritdoc/>
        public override Filter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            object value = null;

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Expected '{{' but found {reader.TokenType}");
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || Filter.FieldJsonPropertyName != reader.GetString())
            {
                throw new JsonException($"Missing {Filter.FieldJsonPropertyName} property.");
            }

            reader.Read();
            string field = reader.GetString();

            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || Filter.OperatorJsonPropertyName != reader.GetString())
            {
                throw new JsonException($@"Missing ""{Filter.OperatorJsonPropertyName}"" property.");
            }

        reader.Read();
        FilterOperator op = Operators[reader.GetString()];
        if (!Filter.UnaryOperators.Contains(op))
        {
            if (reader.Read() && reader.TokenType == JsonTokenType.PropertyName && Filter.ValueJsonPropertyName == reader.GetString())
            {
                reader.Read();
                value = reader.TokenType switch
                {
                    JsonTokenType.Number => reader.GetInt64(),
                    JsonTokenType.String => reader.GetString(),
                    JsonTokenType.False => reader.GetBoolean(),
                    JsonTokenType.True => reader.GetBoolean(),
                    _ => null
                };
            }

                if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Filter json must end with '}'.");
                }
                reader.Read();
            }
            else
            {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    // empty loop to get to the end of the current JSON object
                }
            }

            return new Filter(field, op, value);
        }

#endif

        ///<inheritdoc/>
#if NETSTANDARD1_3
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Filter filter = (Filter)value;
#else
        public override void Write(Utf8JsonWriter writer, Filter value, JsonSerializerOptions options)
        {
            Filter filter = value;
#endif
            writer.WriteStartObject();

            // Field
            writer.WritePropertyName(Filter.FieldJsonPropertyName);
#if NETSTANDARD1_3
            writer.WriteValue(filter.Field);
#else
            writer.WriteStringValue(filter.Field);
#endif

        // operator
        writer.WritePropertyName(Filter.OperatorJsonPropertyName);
        KeyValuePair<string, FilterOperator> kv = Operators.Single(item => item.Value == filter.Operator);

#if NETSTANDARD1_3
            writer.WriteValue(kv.Key);
#else
            writer.WriteStringValue(kv.Key);
#endif

            // value (only if the operator is not an unary operator)
            if (!Filter.UnaryOperators.Contains(filter.Operator))
            {
                writer.WritePropertyName(Filter.ValueJsonPropertyName);
#if NETSTANDARD1_3
                writer.WriteValue(filter.Value);
#else
                writer.WriteStringValue(filter.Value.ToString());
#endif
            }

            writer.WriteEndObject();
        }
    }
}

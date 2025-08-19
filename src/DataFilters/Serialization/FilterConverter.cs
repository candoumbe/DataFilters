using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataFilters.Serialization;

/// <summary>
/// <see cref="JsonConverter"/> implementation that can convert from/to <see cref="Filter"/>
/// </summary>
public class FilterConverter : JsonConverter<Filter>
{
    private static readonly IImmutableDictionary<string, FilterOperator> s_operators = new Dictionary<string, FilterOperator>
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
            throw new JsonException($"""Missing "{Filter.OperatorJsonPropertyName}" property.""");
        }

        reader.Read();
        FilterOperator op = s_operators[reader.GetString()];
        if (!Filter.UnaryOperators.Contains(op))
        {
            if (reader.Read() && reader.TokenType == JsonTokenType.PropertyName && Filter.ValueJsonPropertyName == reader.GetString())
            {
                reader.Read();
                value = reader.TokenType switch
                {
                    JsonTokenType.Number => reader.GetInt64(),
                    JsonTokenType.String => reader.GetString(),
                    JsonTokenType.False or JsonTokenType.True => reader.GetBoolean(),
                    _ => null
                };
            }

            if (!reader.Read() || reader.TokenType is not JsonTokenType.EndObject)
            {
                throw new JsonException("Filter json must end with '}'.");
            }
            reader.Read();
        }
        else
        {
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject)
            {
                // empty loop to get to the end of the current JSON object
            }
        }

        return new Filter(field, op, value);
    }


    ///<inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Filter value, JsonSerializerOptions options)
    {
        Filter filter = value;
        writer.WriteStartObject();

        // Field
        writer.WritePropertyName(Filter.FieldJsonPropertyName);
        writer.WriteStringValue(filter.Field);
        // operator
        writer.WritePropertyName(Filter.OperatorJsonPropertyName);
        KeyValuePair<string, FilterOperator> kv = s_operators.Single(item => item.Value == filter.Operator);

        writer.WriteStringValue(kv.Key);

        // value (only if the operator is not a unary operator)
        if (!Filter.UnaryOperators.Contains(filter.Operator))
        {
            writer.WritePropertyName(Filter.ValueJsonPropertyName);

            writer.WriteStringValue(filter.Value.ToString());
        }

        writer.WriteEndObject();
    }
}
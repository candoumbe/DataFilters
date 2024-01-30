namespace DataFilters.Converters;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
#if NETSTANDARD1_3
using Newtonsoft.Json;
#else
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

/// <summary>
/// <see cref="JsonConverter"/> implementation that can handle (de)serialization of <see cref="FilterOperator"/>
/// from/to JSon .
/// </summary>
#if NETSTANDARD1_3
public class FilterOperatorConverter : JsonConverter
{
#else
public class FilterOperatorConverter : JsonConverter<FilterOperator>
{
#endif
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

#if NETSTANDARD1_3
    /// <inheritdoc/>
    public override bool CanConvert(Type objectType) => typeof(FilterOperator) == objectType;

    /// <inheritdoc/>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.String)
        {
            throw new JsonReaderException();
        }

        string op = reader.ReadAsString();

        return _operators[op.ToLower()];
    }
#else
    /// <inheritdoc/>
    public override FilterOperator Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected {nameof(FilterOperator)} value");
        }

        string op = reader.GetString();
        return _operators[op.ToLower()];
    }
#endif

#if NETSTANDARD1_3
    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        KeyValuePair<string, FilterOperator> result = _operators.Single(op => op.Value == (FilterOperator)value);
        writer.WriteValue(result.Key);
    }
#else

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, FilterOperator value, JsonSerializerOptions options)
    {
#if NETSTANDARD2_0
        string key = _operators.Single(op => op.Value == value).Key;
#else
        (string key, _) = _operators.Single(op => op.Value == value);
#endif
        writer.WriteStringValue(key);
    }
#endif
}

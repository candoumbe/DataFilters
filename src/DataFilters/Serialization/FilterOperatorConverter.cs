using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataFilters.Serialization;

/// <summary>
/// <see cref="JsonConverter"/> implementation that can handle (de)serialization of <see cref="FilterOperator"/>
/// from/to JSon .
/// </summary>
public class FilterOperatorConverter : JsonConverter<FilterOperator>
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

    /// <inheritdoc/>
    public override FilterOperator Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected {nameof(FilterOperator)} value");
        }

        string op = reader.GetString();
        return s_operators[op.ToLower()];
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, FilterOperator value, JsonSerializerOptions options)
    {
        (string key, _) = s_operators.Single(op => op.Value == value);
        writer.WriteStringValue(key);
    }
}
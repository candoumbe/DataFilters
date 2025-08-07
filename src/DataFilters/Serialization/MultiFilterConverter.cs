using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataFilters.Serialization;

/// <summary>
/// <see cref="JsonConverter"/> implementation that allow to convert json string from/to <see cref="MultiFilter"/>
/// </summary>
public class MultiFilterConverter : JsonConverter<MultiFilter>
{
    private readonly FilterConverter _filterConverter;

    /// <summary>
    /// Builds a new <see cref="MultiFilterConverter"/> isntance.
    /// </summary>
    public MultiFilterConverter()
    {
        _filterConverter = new FilterConverter();
    }

    ///<inheritdoc/>
    public override MultiFilter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Multifilter json must start with '{'.");
        }

        reader.Read();

        if (reader.TokenType != JsonTokenType.PropertyName || MultiFilter.LogicJsonPropertyName != reader.GetString())
        {
            throw new JsonException($@"Expected ""{MultiFilter.LogicJsonPropertyName}"" property.");
        }

        reader.Read();
        FilterLogic logic = reader.GetString()?.ToLowerInvariant() switch
        {
            "and" => FilterLogic.And,
            "or" => FilterLogic.Or,
            object value => throw new JsonException(@$"Unexpected ""{value}"" value for ""{MultiFilter.LogicJsonPropertyName}"" property."),
            null => throw new JsonException(@$"Unexpected ""null"" value for ""{MultiFilter.LogicJsonPropertyName}"" property.")

        };

        List<IFilter> filters = [];
        if (reader.Read() && (reader.TokenType != JsonTokenType.PropertyName || MultiFilter.FiltersJsonPropertyName != reader.GetString()))
        {
            throw new JsonException($@"Expected ""{MultiFilter.FiltersJsonPropertyName}"" property.");
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException(@"Expected ""["".");
        }

        reader.Read();

        // We are about to try parsing the JSON to get either a Filter or a MultiFilter.
        // We store a copy of the original reader in case the parsing process fails.
        Utf8JsonReader readerCopy = reader;
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            long position = reader.TokenStartIndex;
            try
            {
                filters.Add(_filterConverter.Read(ref reader, typeof(Filter), options));
            }
            catch
            {
                // The json is not a Filter so we need to go back to where the parsing was performed
                // and try to get a MultFilter instead

                // 1. The copyReader position is moved to where the original parser were before failing
                while (readerCopy.TokenStartIndex < position)
                {
                    readerCopy.Read();
                }

                // 2. Try to parse the Json and get a MultiFilter.
                filters.Add(Read(ref readerCopy, typeof(MultiFilter), options));
                // The parsing was a success -> we move the reader to the continue the parsing process

                while (reader.TokenStartIndex < readerCopy.TokenStartIndex)
                {
                    reader.Read();
                }
                reader.Read(); // Advances the reader until the next token
            }
        }

        reader.Read();
        return reader.TokenType != JsonTokenType.EndObject
            ? throw new JsonException(@"Expected ""}"".")
            : new MultiFilter
            {
                Logic = logic,
                Filters = filters
            };
    }
    ///<inheritdoc/>
    public override void Write(Utf8JsonWriter writer, MultiFilter value, JsonSerializerOptions options)
    {
        MultiFilter mf = value;
        writer.WriteStartObject();

        writer.WritePropertyName(MultiFilter.LogicJsonPropertyName);
        writer.WriteStringValue(mf.Logic.ToString().ToLower());

        writer.WritePropertyName(MultiFilter.FiltersJsonPropertyName);
        writer.WriteStartArray();
        foreach (IFilter filter in mf.Filters)
        {
            if (filter is Filter f)
            {
                _filterConverter.Write(writer, f, options);
            }
            else if (filter is MultiFilter multiFilter)
            {
                Write(writer, multiFilter, options);
            }
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
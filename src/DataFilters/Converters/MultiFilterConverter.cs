using System;
using System.Collections.Generic;
using System.Diagnostics;

#if NETSTANDARD1_3
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#else
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace DataFilters.Converters
{
    /// <summary>
    /// <see cref="JsonConverter"/> implementation that allow to convert json string from/to <see cref="MultiFilter"/>
    /// </summary>
#if NETSTANDARD1_3
    public class MultiFilterConverter : JsonConverter
    {
        private static readonly IImmutableDictionary<string, FilterLogic> _logics = new Dictionary<string, FilterLogic>
        {
            [nameof(FilterLogic.And).ToLower()] = FilterLogic.And,
            [nameof(FilterLogic.Or).ToLower()] = FilterLogic.Or
        }.ToImmutableDictionary();

        public override bool CanConvert(Type objectType) => objectType == typeof(MultiFilter);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            MultiFilter multiFilter = null;

            JToken token = JToken.ReadFrom(reader);
            if (objectType == typeof(MultiFilter) && token.Type == JTokenType.Object)
            {
                IEnumerable<JProperty> properties = ((JObject)token).Properties();

                JProperty logicProperty = properties.SingleOrDefault(prop => prop.Name == MultiFilter.LogicJsonPropertyName);

                if (logicProperty != null)
                {
                    JProperty filtersProperty = properties.SingleOrDefault(prop => prop.Name == MultiFilter.FiltersJsonPropertyName);
                    if (filtersProperty is JProperty prop && filtersProperty.Value.Type == JTokenType.Array)
                    {
                        JArray filtersArray = token[MultiFilter.FiltersJsonPropertyName].Value<JArray>();
                        int nbFilters = filtersArray.Count();
                        if (nbFilters >= 2)
                        {
                            IList<IFilter> filters = new List<IFilter>(nbFilters);
                            foreach (JToken item in filtersArray)
                            {
                                IFilter kf = (IFilter)item.ToObject<Filter>() ?? item.ToObject<MultiFilter>();
                                filters.Add(kf);
                            }

                            multiFilter = new MultiFilter
                            {
                                Logic = _logics[token[MultiFilter.LogicJsonPropertyName].Value<string>()],
                                Filters = filters
                            };
                        }
                    }
                }
            }

            return multiFilter?.As(objectType);
        }
#else
    public class MultiFilterConverter : JsonConverter<MultiFilter>
    {
        private readonly FilterConverter _filterConverter;

        public MultiFilterConverter()
        {
            _filterConverter = new FilterConverter();
        }

        public override MultiFilter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var clone = reader;
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

            IList<IFilter> filters = new List<IFilter>();
            if (reader.Read() && (reader.TokenType != JsonTokenType.PropertyName || MultiFilter.FiltersJsonPropertyName != reader.GetString()))
            {
                throw new JsonException($@"Expected ""{MultiFilter.FiltersJsonPropertyName}"" property.");
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($@"Expected ""["".");
            }

            reader.Read();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                long position = reader.TokenStartIndex;
                try
                {
                    filters.Add(_filterConverter.Read(ref reader, typeof(Filter), options));
                }
                catch
                {
                    // this allow the clone start reading where the reader crashed
                    while(clone.TokenStartIndex < position)
                    {
                        clone.Read();
                    }
                    filters.Add(Read(ref clone, typeof(MultiFilter), options));
                    // This loop allows the reader where the clone ends reading
                    while (reader.TokenStartIndex < clone.TokenStartIndex)
                    {
                        reader.Read();
                    }
                    reader.Read(); // Advances the reader onto the next token
                }
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException($@"Expected ""}}"".");
            }

            return new MultiFilter
            {
                Logic = logic,
                Filters = filters
            };
        }
#endif
        ///<inheritdoc/>
#if NETSTANDARD1_3
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            MultiFilter mf = (MultiFilter)value;
#else
        public override void Write(Utf8JsonWriter writer, MultiFilter mf, JsonSerializerOptions options)
        {
#endif
            writer.WriteStartObject();

            writer.WritePropertyName(MultiFilter.LogicJsonPropertyName);
#if NETSTANDARD1_3
            writer.WriteValue(mf.Logic.ToString().ToLower());
#else
            writer.WriteStringValue(mf.Logic.ToString().ToLower());
#endif

            writer.WritePropertyName(MultiFilter.FiltersJsonPropertyName);
            writer.WriteStartArray();
            foreach (IFilter filter in mf.Filters)
            {
#if NETSTANDARD1_3
                serializer.Serialize(writer, filter);
#else
                if (filter is Filter f)
                {
                    _filterConverter.Write(writer, f, options);
                }
                else if (filter is MultiFilter multiFilter)
                {
                    Write(writer, multiFilter, options);
                }
#endif
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}

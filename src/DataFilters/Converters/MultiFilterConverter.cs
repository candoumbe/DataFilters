using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DataFilters.Converters
{
    /// <summary>
    /// <see cref="JsonConverter"/> implementation that allow to convert json string from/to <see cref="MultiFilter"/>
    /// </summary>
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
            MultiFilter kcf = null;

            JToken token = JToken.ReadFrom(reader);
            if (objectType == typeof(MultiFilter) && token.Type == JTokenType.Object)
            {
                IEnumerable<JProperty> properties = ((JObject)token).Properties();

                JProperty logicProperty = properties
                    .SingleOrDefault(prop => prop.Name == MultiFilter.LogicJsonPropertyName);

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

                            kcf = new MultiFilter
                            {
                                Logic = _logics[token[MultiFilter.LogicJsonPropertyName].Value<string>()],
                                Filters = filters
                            };
                        }
                    }
                }
            }

            return kcf?.As(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            MultiFilter kcf = (MultiFilter)value;

            writer.WriteStartObject();

            // TODO Maybe can we rely on the serializer to handle the logic serialization ?
            writer.WritePropertyName(MultiFilter.LogicJsonPropertyName);
            writer.WriteValue(kcf.Logic.ToString().ToLower());

            writer.WritePropertyName(MultiFilter.FiltersJsonPropertyName);
            writer.WriteStartArray();
            foreach (IFilter filter in kcf.Filters)
            {
                serializer.Serialize(writer, filter);
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}

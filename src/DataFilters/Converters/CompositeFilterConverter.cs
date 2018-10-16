using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DataFilters.Converters
{
    /// <summary>
    /// <see cref="JsonConverter"/> implementation that allow to convert json string from/to <see cref="CompositeFilter"/>
    /// </summary>
    public class CompositeFilterConverter : JsonConverter
    {

        private static IImmutableDictionary<string, FilterLogic> Logics = new Dictionary<string, FilterLogic>
        {
            [FilterLogic.And.ToString().ToLower()] = FilterLogic.And,
            [FilterLogic.Or.ToString().ToLower()] = FilterLogic.Or
        }.ToImmutableDictionary();


        private static IImmutableDictionary<string, FilterOperator> Operators = new Dictionary<string, FilterOperator>
        {
            ["contains"] = FilterOperator.Contains,
            ["endswith"] = FilterOperator.EndsWith,
            ["eq"] = FilterOperator.EqualTo,
            ["gt"] = FilterOperator.GreaterThan,
            ["gte"] = FilterOperator.GreaterThanOrEqual,
            ["isempty"] = FilterOperator.IsEmpty,
            ["isnotempty"] = FilterOperator.IsNotEmpty,
            ["isnotnull"] = FilterOperator.IsNotNull,
            ["isnull"] = FilterOperator.IsNull,
            ["lt"] = FilterOperator.LessThan,
            ["lte"] = FilterOperator.LessThanOrEqualTo,
            ["neq"] = FilterOperator.NotEqualTo,
            ["startswith"] = FilterOperator.StartsWith,

        }.ToImmutableDictionary();

        public override bool CanConvert(Type objectType) => objectType == typeof(CompositeFilter);


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            CompositeFilter kcf = null;

            JToken token = JToken.ReadFrom(reader);
            if (objectType == typeof(CompositeFilter))
            {
                if (token.Type == JTokenType.Object)
                {
                    IEnumerable<JProperty> properties = ((JObject)token).Properties();

                    JProperty logicProperty = properties
                        .SingleOrDefault(prop => prop.Name == CompositeFilter.LogicJsonPropertyName);

                    if (logicProperty != null)
                    {
                        JProperty filtersProperty = properties.SingleOrDefault(prop => prop.Name == CompositeFilter.FiltersJsonPropertyName);
                        if (filtersProperty != null
                            && filtersProperty.Type == JTokenType.Array)
                        {
                            JArray filtersArray = (JArray)token[CompositeFilter.FiltersJsonPropertyName];
                            int nbFilters = filtersArray.Count();
                            if (nbFilters > 2)
                            {
                                IList<IFilter> filters = new List<IFilter>(nbFilters);
                                foreach (JToken item in filtersArray)
                                {
                                    IFilter kf = (IFilter)item.ToObject<Filter>() ?? item.ToObject<CompositeFilter>();

                                    if (kf != null)
                                    {
                                        filters.Add(kf);
                                    }
                                }


                                if (filters.Count() >= 2)
                                {
                                    kcf = new CompositeFilter
                                    {
                                        Logic = Logics[token[CompositeFilter.LogicJsonPropertyName].Value<string>()],
                                        Filters = filters
                                    };
                                }
                            }

                        }
                    }
                }
            }



            return kcf?.As(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            CompositeFilter kcf = (CompositeFilter)value;

            writer.WriteStartObject();

            // TODO Maybe can we rely on the serializer to handle the logic serialization ?
            writer.WritePropertyName(CompositeFilter.LogicJsonPropertyName);
            writer.WriteValue(kcf.Logic.ToString().ToLower());

            writer.WritePropertyName(CompositeFilter.FiltersJsonPropertyName);
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

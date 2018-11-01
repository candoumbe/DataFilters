using Newtonsoft.Json;
#if !NETSTANDARD1_0
using Newtonsoft.Json.Schema;
#endif
using System.Collections.Generic;
using System.Linq;
using static Newtonsoft.Json.Required;
using static Newtonsoft.Json.DefaultValueHandling;
using static Newtonsoft.Json.JsonConvert;
using DataFilters.Converters;

namespace DataFilters
{
    /// <summary>
    /// An instance of this class holds combination of <see cref="IDataFilter"/>
    /// </summary>
    [JsonObject]
    [JsonConverter(typeof(CompositeFilterConverter))]
    public class CompositeFilter : IFilter
    {
        /// <summary>
        /// Name of the json property that holds filter's filters collection.
        /// </summary>
        public const string FiltersJsonPropertyName = "filters";

        /// <summary>
        /// Name of the json property that holds the composite filter's logic
        /// </summary>
        public const string LogicJsonPropertyName = "logic";

#if !NETSTANDARD1_0
        public static JSchema Schema => new JSchema
        {
            Type = JSchemaType.Object,
            Properties =
            {
                [FiltersJsonPropertyName] = new JSchema { Type = JSchemaType.Array, MinimumItems = 2 },
                [LogicJsonPropertyName] = new JSchema { Type = JSchemaType.String, Default = "and"}
            },
            Required = { FiltersJsonPropertyName },
            AllowAdditionalProperties = false
        };
#endif

        /// <summary>
        /// Collections of filters
        /// </summary>
        [JsonProperty(PropertyName = FiltersJsonPropertyName, Required = Always)]
        public IEnumerable<IFilter> Filters { get; set; } = Enumerable.Empty<IFilter>();

        /// <summary>
        /// Operator to apply between <see cref="Filters"/>
        /// </summary>
        [JsonProperty(PropertyName = LogicJsonPropertyName, DefaultValueHandling = IgnoreAndPopulate)]
        [JsonConverter(typeof(CamelCaseEnumTypeConverter))]
        public FilterLogic Logic { get; set; } = FilterLogic.And;

        public virtual string ToJson()
#if DEBUG
        => SerializeObject(this, Formatting.Indented);
#else
            => SerializeObject(this);
#endif

#if DEBUG
        public override string ToString() => ToJson();
#endif

        public IFilter Negate()
        {
            CompositeFilter filter = new CompositeFilter
            {
                Logic = Logic == FilterLogic.And
                    ? FilterLogic.Or
                    : FilterLogic.And,
                Filters = Filters.Select(f => f.Negate())
#if DEBUG
                .ToArray()
#endif
            };

            return filter;
        }

    }
}

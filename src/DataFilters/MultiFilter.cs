using DataFilters.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using static Newtonsoft.Json.DefaultValueHandling;
using static Newtonsoft.Json.Required;

namespace DataFilters
{
    /// <summary>
    /// An instance of this class holds combination of <see cref="IFilter"/>
    /// </summary>
    [JsonObject]
    [JsonConverter(typeof(MultiFilterConverter))]
    public class MultiFilter : IFilter, IEquatable<MultiFilter>
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
        public FilterLogic Logic { get; set; }

        public virtual string ToJson() => this.Jsonify();

        public override string ToString() => this.Jsonify();

        public IFilter Negate()
        {
            MultiFilter filter = new MultiFilter
            {
                Logic = Logic switch
                {
                    FilterLogic.And => FilterLogic.Or,
                    FilterLogic.Or => FilterLogic.And,
                    _ => throw new ArgumentOutOfRangeException($"Unsupported {Logic}")
                },
                Filters = Filters.Select(f => f.Negate())
#if DEBUG
                .ToArray()
#endif
            };

            return filter;
        }

#if NETSTANDARD1_3 || NETSTANDARD2_0
        public override int GetHashCode() => (Logic, Filters).GetHashCode();
#else
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Logic);
            foreach (IFilter filter in Filters)
            {
                hash.Add(filter);
            }
            return hash.ToHashCode();
        }
#endif

        public bool Equals(IFilter other) => Equals(other as MultiFilter);

        public override bool Equals(object obj) => Equals(obj as MultiFilter);

        public bool Equals(MultiFilter other)
            => Logic == other?.Logic
            && Filters.Count() == other?.Filters?.Count()
            && Filters.All(filter => other?.Filters?.Contains(filter) ?? false)
            && (other?.Filters.All(filter => Filters.Contains(filter)) ?? false);
    }
}

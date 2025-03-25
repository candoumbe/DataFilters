namespace DataFilters
{
    using DataFilters.Converters;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Schema;
    using System;
    using System.Collections.Generic;
    using System.Linq;

#if !NETSTANDARD1_3
    using System.Text.Json.Serialization;
#else
    using static Newtonsoft.Json.DefaultValueHandling;
    using static Newtonsoft.Json.Required;
#endif

    /// <summary>
    /// An instance of this class holds combination of <see cref="IFilter"/>
    /// </summary>
    [JsonObject]
#if NETSTANDARD1_3
    [JsonConverter(typeof(MultiFilterConverter))]
#else
    [System.Text.Json.Serialization.JsonConverter(typeof(MultiFilterConverter))]
#endif
    public sealed class MultiFilter : IFilter, IEquatable<MultiFilter>
    {
        /// <summary>
        /// Name of the json property that holds filter's filters collection.
        /// </summary>
        public const string FiltersJsonPropertyName = "filters";

        /// <summary>
        /// Name of the json property that holds the composite filter's logic
        /// </summary>
        public const string LogicJsonPropertyName = "logic";

        /// <summary>
        /// <see cref="MultiFilter"/> JSON schema
        /// </summary>
        public static JSchema Schema => new()
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

        /// <summary>
        /// Collections of filters
        /// </summary>
#if NETSTANDARD1_3
        [JsonProperty(PropertyName = FiltersJsonPropertyName, Required = Always)]
#else
        [JsonPropertyName(FiltersJsonPropertyName)]
#endif
        public IEnumerable<IFilter> Filters { get; set; } = [];

        /// <summary>
        /// Operator to apply between <see cref="Filters"/>
        /// </summary>
#if NETSTANDARD1_3
        [JsonProperty(PropertyName = LogicJsonPropertyName, DefaultValueHandling = IgnoreAndPopulate)]
        [JsonConverter(typeof(CamelCaseEnumTypeConverter))]
#else
        [JsonPropertyName(LogicJsonPropertyName)]
#endif
        public FilterLogic Logic { get; set; }

        ///<inheritdoc/>
        public string ToJson() => this.Jsonify();

        ///<inheritdoc/>
        public IFilter Negate()
        {
            MultiFilter filter = new()
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

        ///<inheritdoc/>
#if NETSTANDARD1_3 || NETSTANDARD2_0
        public override int GetHashCode() => (Logic, Filters).GetHashCode();
#else
        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Logic);
            foreach (IFilter filter in Filters)
            {
                hash.Add(filter);
            }
            return hash.ToHashCode();
        }
#endif

        ///<inheritdoc/>
        public bool Equals(IFilter other) => Equals(other as MultiFilter);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as MultiFilter);

        ///<inheritdoc/>
        public bool Equals(MultiFilter other)
            => Logic == other?.Logic
            && Filters.Count() == other?.Filters?.Count()
            && Filters.All(filter => other?.Filters?.Contains(filter) ?? false)
            && (other?.Filters.All(filter => Filters.Contains(filter)) ?? false);

        ///<inheritdoc/>
        public override string ToString() => this.Jsonify();
    }
}

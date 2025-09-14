using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using DataFilters.Serialization;
using Newtonsoft.Json.Schema;

namespace DataFilters;

/// <summary>
/// An instance of this class holds combination of <see cref="IFilter"/>
/// </summary>
[JsonConverter(typeof(MultiFilterConverter))]
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
    [JsonPropertyName(FiltersJsonPropertyName)]
    public IEnumerable<IFilter> Filters { get; set; } = [];

    /// <summary>
    /// Operator to apply between <see cref="Filters"/>
    /// </summary>
    [JsonPropertyName(LogicJsonPropertyName)]
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
                _ => throw new NotSupportedException($"Unsupported {Logic}")
            },
            Filters = [.. Filters.Select(f => f.Negate())]
        };

        return filter;
    }

    ///<inheritdoc/>
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
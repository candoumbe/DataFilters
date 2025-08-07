using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DataFilters.Serialization;
using Newtonsoft.Json.Schema;

namespace DataFilters;
/// <summary>
/// An instance of this class holds a filter
/// </summary>
[JsonConverter(typeof(FilterConverter))]
public sealed class Filter : IFilter, IEquatable<Filter>
{
    /// <summary>
    /// Filter that always returns <c>true</c>
    /// </summary>
    public static Filter True => new(null, default);

    /// <summary>
    /// Pattern that field name should respect.
    /// </summary>
    /// #lang : regex
    /// language=regex
    public const string ValidFieldNamePattern = """
                                                [a-zA-Z_]+((\["[a-zA-Z0-9_]+"]|(\.[a-zA-Z0-9_]+))*)
                                                """;

    /// <summary>
    /// Regular expression used to validate
    /// </summary>
    /// <returns></returns>
    public static readonly Regex ValidFieldNameRegex = new(ValidFieldNamePattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

    /// <summary>
    /// Name of the json property that holds the field name
    /// </summary>
    public const string FieldJsonPropertyName = "field";

    /// <summary>
    /// Name of the json property that holds the operator
    /// </summary>
    public const string OperatorJsonPropertyName = "op";

    /// <summary>
    /// Name of the json property that holds the value
    /// </summary>
    public const string ValueJsonPropertyName = "value";

    /// <summary>
    /// <see cref="FilterOperator"/>s that required <see cref="Value"/> to be null.
    /// </summary>
    public static ISet<FilterOperator> UnaryOperators { get; } = new HashSet<FilterOperator> { FilterOperator.IsEmpty, FilterOperator.IsNotEmpty, FilterOperator.IsNotNull, FilterOperator.IsNull };

    /// <summary>
    /// Generates the <see cref="JSchema"/> for the specified <see cref="FilterOperator"/>.
    /// </summary>
    /// <param name="op"></param>
    /// <returns></returns>
    public static JSchema Schema(FilterOperator op)
    {
        JSchema schema = op switch
        {
            FilterOperator.Contains or FilterOperator.StartsWith or FilterOperator.EndsWith => new JSchema
            {
                Type = JSchemaType.Object,
                Properties =
                {
                    [FieldJsonPropertyName] = new JSchema { Type = JSchemaType.String },
                    [OperatorJsonPropertyName] = new JSchema { Type = JSchemaType.String },
                    [ValueJsonPropertyName] = new JSchema { Type = JSchemaType.String }
                },
                Required = { FieldJsonPropertyName, OperatorJsonPropertyName }
            },
            FilterOperator.IsEmpty or FilterOperator.IsNotEmpty or FilterOperator.IsNotNull or FilterOperator.IsNull => new JSchema
            {
                Type = JSchemaType.Object,
                Properties =
                {
                    [FieldJsonPropertyName] = new JSchema { Type = JSchemaType.String },
                    [OperatorJsonPropertyName] = new JSchema { Type = JSchemaType.String }
                },
                Required = { FieldJsonPropertyName, OperatorJsonPropertyName }
            },
            _ => new JSchema
            {
                Type = JSchemaType.Object,
                Properties =
                {
                    [FieldJsonPropertyName] = new JSchema { Type = JSchemaType.String,  },
                    [OperatorJsonPropertyName] = new JSchema { Type = JSchemaType.String },
                    [ValueJsonPropertyName] = new JSchema {
                        Not = new JSchema() { Type = JSchemaType.Null }
                    }
                },
                Required = { FieldJsonPropertyName, OperatorJsonPropertyName, ValueJsonPropertyName }
            },
        };
        schema.AllowAdditionalProperties = false;

        return schema;
    }

    /// <summary>
    /// Name of the field  the filter will be applied to
    /// </summary>
    [JsonPropertyName(FieldJsonPropertyName)]
    public string Field { get; }

    /// <summary>
    /// Operator to apply to the filter
    /// </summary>
    [JsonPropertyName(OperatorJsonPropertyName)]
    public FilterOperator Operator { get; }

    /// <summary>
    /// Value of the filter
    /// </summary>
    [JsonPropertyName(ValueJsonPropertyName)]
    public object Value { get; }

    /// <summary>
    /// Builds a new <see cref="Filter"/> instance.
    /// </summary>
    /// <param name="field">name of the field</param>
    /// <param name="operator"><see cref="Filter"/> to apply</param>
    /// <param name="value">value of the filter</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="field"/> does not conform with <see cref="ValidFieldNamePattern"/></exception>
    public Filter(string field, FilterOperator @operator, object value = null)
    {
        if (!string.IsNullOrEmpty(field) && !ValidFieldNameRegex.IsMatch(field))
        {
            throw new ArgumentOutOfRangeException(nameof(field), field, $"field name is not valid ({ValidFieldNamePattern}).");
        }

        Field = field;
        switch (@operator)
        {
            case FilterOperator.EqualTo when value is null:
                Operator = FilterOperator.IsNull;
                break;
            case FilterOperator.NotEqualTo when value is null:
                Operator = FilterOperator.IsNotNull;
                break;
            default:
                Operator = @operator;
                Value = value;
                break;
        }
    }

    ///<inheritdoc/>
    public string ToJson() => this.Jsonify();

    ///<inheritdoc/>
    public override string ToString() => ToJson();

    ///<inheritdoc/>
    public bool Equals(Filter other)
        => other != null
           && (ReferenceEquals(other, this)
               || (Equals(other.Field, Field) && Equals(other.Operator, Operator) && Equals(other.Value, Value)));

    ///<inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as Filter);

    ///<inheritdoc/>
    public bool Equals(IFilter other) => Equals(other as Filter);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Field, Operator, Value);

    ///<inheritdoc/>
    public IFilter Negate()
    {
        FilterOperator @operator = Operator switch
        {
            FilterOperator.EqualTo => FilterOperator.NotEqualTo,
            FilterOperator.NotEqualTo => FilterOperator.EqualTo,
            FilterOperator.IsNull => FilterOperator.IsNotNull,
            FilterOperator.IsNotNull => FilterOperator.IsNull,
            FilterOperator.LessThan => FilterOperator.GreaterThan,
            FilterOperator.GreaterThan => FilterOperator.LessThan,
            FilterOperator.GreaterThanOrEqual => FilterOperator.LessThanOrEqualTo,
            FilterOperator.StartsWith => FilterOperator.NotStartsWith,
            FilterOperator.NotStartsWith => FilterOperator.StartsWith,
            FilterOperator.EndsWith => FilterOperator.NotEndsWith,
            FilterOperator.NotEndsWith => FilterOperator.EndsWith,
            FilterOperator.Contains => FilterOperator.NotContains,
            FilterOperator.NotContains => FilterOperator.Contains,
            FilterOperator.IsEmpty => FilterOperator.IsNotEmpty,
            FilterOperator.IsNotEmpty => FilterOperator.IsEmpty,
            FilterOperator.LessThanOrEqualTo => FilterOperator.GreaterThanOrEqual,
            _ => throw new NotSupportedException("Unknown operator"),
        };
        return new Filter(Field, @operator, Value);
    }

    ///<inheritdoc/>
    public void Deconstruct(out string field, out FilterOperator @operator, out object value)
    {
        field = Field;
        @operator = Operator;
        value = Value;
    }
}
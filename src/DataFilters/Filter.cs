using DataFilters.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using static Newtonsoft.Json.DefaultValueHandling;
using static Newtonsoft.Json.JsonConvert;
using static Newtonsoft.Json.Required;

namespace DataFilters
{
    /// <summary>
    /// An instance of this class holds a kendo filter
    /// </summary>
    [JsonObject]
    [JsonConverter(typeof(FilterConverter))]
    public class Filter : IFilter, IEquatable<Filter>
    {
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
        public static IEnumerable<FilterOperator> UnaryOperators { get; } = new[]{
            FilterOperator.IsEmpty,
            FilterOperator.IsNotEmpty,
            FilterOperator.IsNotNull,
            FilterOperator.IsNull
        };

        /// <summary>
        /// Builds a new <see cref="Filter"/> instance.
        /// </summary>
        /// <param name="field">name of the field</param>
        /// <param name="operator"><see cref="Filter"/> to apply</param>
        /// <param name="value">value of the filter</param>
        public Filter(string field, FilterOperator @operator, object value = null)
        {
            Field = field;
            if (@operator == FilterOperator.EqualTo && value == null)
            {
                Operator = FilterOperator.IsNull;
            }
            else
            {
                Operator = @operator;
                Value = value;
            }
        }

#if !NETSTANDARD1_0
        /// <summary>
        /// Generates the <see cref="JSchema"/> for the specified <see cref="FilterOperator"/>.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static JSchema Schema(FilterOperator op)
        {
            JSchema schema;
            switch (op)
            {
                case FilterOperator.Contains:
                case FilterOperator.StartsWith:
                case FilterOperator.EndsWith:
                    schema = new JSchema
                    {
                        Type = JSchemaType.Object,
                        Properties =
                        {
                            [FieldJsonPropertyName] = new JSchema { Type = JSchemaType.String },
                            [OperatorJsonPropertyName] = new JSchema { Type = JSchemaType.String },
                            [ValueJsonPropertyName] = new JSchema { Type = JSchemaType.String }
                        },
                        Required = { FieldJsonPropertyName, OperatorJsonPropertyName }
                    };
                    break;
                case FilterOperator.IsEmpty:
                case FilterOperator.IsNotEmpty:
                case FilterOperator.IsNotNull:
                case FilterOperator.IsNull:
                    schema = new JSchema
                    {
                        Type = JSchemaType.Object,
                        Properties =
                        {
                            [FieldJsonPropertyName] = new JSchema { Type = JSchemaType.String },
                            [OperatorJsonPropertyName] = new JSchema { Type = JSchemaType.String }
                        },
                        Required = { FieldJsonPropertyName, OperatorJsonPropertyName }

                    };
                    break;
                default:
                    schema = new JSchema
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
                    };
                    break;
            }
            schema.AllowAdditionalProperties = false;

            return schema;
        }
#endif

        /// <summary>
        /// Name of the field to filter
        /// </summary>
        [JsonProperty(FieldJsonPropertyName, Required = Always)]
        public string Field { get; }

        /// <summary>
        /// Operator to apply to the filter
        /// </summary>
        [JsonProperty(OperatorJsonPropertyName, Required = Always)]
        //[JsonConverter(typeof(DataFilterOperatorConverter))]
        public FilterOperator Operator { get; }

        /// <summary>
        /// Value of the filter
        /// </summary>
        [JsonProperty(ValueJsonPropertyName,
            Required = AllowNull,
            DefaultValueHandling = IgnoreAndPopulate,
            NullValueHandling = NullValueHandling.Ignore)]
        public object Value { get; }

        public virtual string ToJson()
#if DEBUG
        => SerializeObject(this, Formatting.Indented);
#else
            => SerializeObject(this);
#endif

#if DEBUG
        public override string ToString() => ToJson();
#endif

        public bool Equals(Filter other)
            => other != null
            && (ReferenceEquals(other, this)
            || (Equals(other.Field, Field) && Equals(other.Operator, Operator) && Equals(other.Value, Value)));

        public override bool Equals(object obj) => Equals(obj as Filter);

        public override int GetHashCode() => (Field, Operator, Value).GetHashCode();

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
                FilterOperator.IsEmpty => FilterOperator.IsNotEmpty,
                FilterOperator.IsNotEmpty => FilterOperator.IsEmpty,
                FilterOperator.LessThanOrEqualTo => FilterOperator.GreaterThanOrEqual,
                _ => throw new ArgumentOutOfRangeException(nameof(Operator), "Unknown operator"),
            };
            return new Filter(Field, @operator, Value);
        }

        public bool Equals(IFilter other) => Equals(other as Filter);

        public void Deconstruct(out string field,  out FilterOperator @operator, out object value)
        {
            field = Field;
            @operator = Operator;
            value = Value;
        }
    }
}

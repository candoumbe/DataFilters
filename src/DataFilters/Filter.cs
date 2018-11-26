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

        public IFilter Negate()
        {
            FilterOperator @operator;
            switch (Operator)
            {
                case FilterOperator.EqualTo:
                    @operator = FilterOperator.NotEqualTo;
                    break;
                case FilterOperator.NotEqualTo:
                    @operator = FilterOperator.EqualTo;
                    break;
                case FilterOperator.IsNull:
                    @operator = FilterOperator.IsNotNull;
                    break;
                case FilterOperator.IsNotNull:
                    @operator = FilterOperator.IsNull;
                    break;
                case FilterOperator.LessThan:
                    @operator = FilterOperator.GreaterThan;
                    break;
                case FilterOperator.GreaterThan:
                    @operator = FilterOperator.LessThan;
                    break;
                case FilterOperator.GreaterThanOrEqual:
                    @operator = FilterOperator.LessThanOrEqualTo;
                    break;
                case FilterOperator.StartsWith:
                    @operator = FilterOperator.NotStartsWith;
                    break;
                case FilterOperator.NotStartsWith:
                    @operator = FilterOperator.StartsWith;
                    break;
                case FilterOperator.EndsWith:
                    @operator = FilterOperator.NotEndsWith;
                    break;
                case FilterOperator.NotEndsWith:
                    @operator = FilterOperator.EndsWith;
                    break;
                case FilterOperator.Contains:
                    @operator = FilterOperator.NotContains;
                    break;
                case FilterOperator.IsEmpty:
                    @operator = FilterOperator.IsNotEmpty;
                    break;
                case FilterOperator.IsNotEmpty:
                    @operator = FilterOperator.IsEmpty;
                    break;
                case FilterOperator.LessThanOrEqualTo:
                    @operator = FilterOperator.GreaterThanOrEqual;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@operator), "Unknown @operator");
            }

            return new Filter(Field, @operator, Value);
        }
    }
}

namespace DataFilters;

using Converters;

#if NETSTANDARD1_3
using Newtonsoft.Json;
#else
using System.Text.Json.Serialization;
#endif
/// <summary>
/// Operators that can be used when building <see cref="Filter"/> instances.
/// </summary>
[JsonConverter(typeof(FilterOperatorConverter))]
public enum FilterOperator : short
{
    /// <summary>
    /// <c>=</c> operator
    /// </summary>
    EqualTo,

    /// <summary>
    /// <c>!=</c> operator
    /// </summary>
    NotEqualTo,

    /// <summary>
    /// <c>is null</c> operator
    /// </summary>
    IsNull,

    /// <summary>
    /// <c>is not null</c> operator
    /// </summary>
    IsNotNull,

    /// <summary>
    /// <c>&lt;</c> operator
    /// </summary>
    LessThan,

    /// <summary>
    /// <c>&gt;</c> operator
    /// </summary>
    GreaterThan,

    /// <summary>
    /// <c>&gt;=</c> operator
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Applies only to string
    /// </summary>
    StartsWith,

    /// <summary>
    /// Applies only to string
    /// </summary>
    NotStartsWith,

    /// <summary>
    /// <remarks>Applies only to string</remarks>
    /// </summary>
    EndsWith,

    /// <summary>
    /// <remarks>Applies only to string</remarks>
    /// </summary>
    NotEndsWith,

    /// <summary>
    /// "Contains" operator
    /// </summary>
    Contains,

    /// <summary>
    /// opposite of <see cref="Contains"/> operator
    /// </summary>
    NotContains,

    /// <summary>
    /// <c>isempty</c> operator
    /// </summary>
    IsEmpty,

    /// <summary>
    /// Opposite of <see cref="IsEmpty"/> operator
    /// </summary>
    IsNotEmpty,

    /// <summary>
    /// <c>&lt;=</c> operator
    /// </summary>
    LessThanOrEqualTo
}

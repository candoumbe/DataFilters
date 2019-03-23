namespace DataFilters
{
    /// <summary>
    /// Operators that can be used when building <see cref="Filter"/> instances.
    /// </summary>
    public enum FilterOperator : short
    {
        EqualTo,

        NotEqualTo,

        IsNull,

        IsNotNull,

        LessThan,

        GreaterThan,

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
        /// 
        /// <remarks>Applies only to string</remarks>
        /// </summary>
        EndsWith,

        /// <summary>
        /// 
        /// <remarks>Applies only to string</remarks>
        /// </summary>
        NotEndsWith,

        Contains,

        NotContains,

        IsEmpty,

        IsNotEmpty,

        LessThanOrEqualTo
    }
}

namespace DataFilters
{
    /// <summary>
    /// Operators that can be used when building <see cref="Filter"/> instances.
    /// </summary>
    public enum FilterOperator
    {
        /// <summary>
        /// 
        /// </summary>
        EqualTo,
        /// <summary>

        /// </summary>
        NotEqualTo,

        IsNull,

        IsNotNull,

        LessThan,

        GreaterThan,

        GreaterThanOrEqual,

        /// <summary
        /// Applies only to string
        /// </summary>
        StartsWith,

        /// <summary>
        /// 
        /// <remarks>Applies only to string</remarks>
        /// </summary>
        EndsWith,

        Contains,

        IsEmpty,

        IsNotEmpty,

        LessThanOrEqualTo
    }
}

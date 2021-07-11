namespace DataFilters.Converters
{
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Converter that specifies that enum members should be converted using CamelCaseNamingStrategy
    /// </summary>
    public class CamelCaseEnumTypeConverter : StringEnumConverter
    {
        /// <summary>
        /// Builds a new <see cref="CamelCaseEnumTypeConverter"/> instance
        /// </summary>
        public CamelCaseEnumTypeConverter() => NamingStrategy = new CamelCaseNamingStrategy();
    }
}

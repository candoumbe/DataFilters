using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFilters.Converters
{
    /// <summary>
    /// Converter that specifies that enum
    /// </summary>
    public class CamelCaseEnumTypeConverter : StringEnumConverter
    {
        /// <summary>
        /// Builds a new <see cref="CamelCaseEnumTypeConverter"/> instance
        /// </summary>

        public CamelCaseEnumTypeConverter() => NamingStrategy = new CamelCaseNamingStrategy();
    }
}

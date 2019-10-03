using Newtonsoft.Json.Converters;
#if !NETSTANDARD1_3
using Newtonsoft.Json.Serialization;
#endif
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
#if NETSTANDARD1_3
        public CamelCaseEnumTypeConverter() => CamelCaseText = true;
#else
        public CamelCaseEnumTypeConverter() => NamingStrategy = new CamelCaseNamingStrategy();
#endif
    }
}

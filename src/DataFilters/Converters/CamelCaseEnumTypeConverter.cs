using Newtonsoft.Json.Converters;
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
        public CamelCaseEnumTypeConverter()
        {
            CamelCaseText = true;
        }
    }
}

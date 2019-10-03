using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFilters
{
    public class InvalidSortExpression : Exception
    {
        public InvalidSortExpression(string expression) : base($"'{expression}' must matches '{SortValidator.Pattern}' pattern")
        {

        }

        public InvalidSortExpression() : base()
        {
        }

#if NETSTANDARD2_0 || NETSTANDARD2_1
        protected InvalidSortExpression(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
#endif

        public InvalidSortExpression(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

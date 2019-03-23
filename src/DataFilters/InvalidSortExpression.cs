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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFilters
{
    /// <summary>
    /// Logic that can be apply when combining <see cref="Filter"/>s.
    /// </summary>
    public enum FilterLogic
    {
        /// <summary>
        /// Logical AND operator will be applied to all
        /// </summary>
        And,
        /// <summary>
        /// Logicial OR operatior will be applied 
        /// </summary>
        Or
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataFilters
{
    /// <summary>
    /// Constants used throughout the package
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Ragex pattern that field name should respect.
        /// </summary>
        public const string ValidFieldNamePattern = @"[a-zA-Z_]+((\[""[a-zA-Z0-9_]+""]|(\.[a-zA-Z0-9_]+))*)";
    }
}

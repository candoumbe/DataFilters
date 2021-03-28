using System;
using System.Diagnostics.CodeAnalysis;

namespace DataFilters.TestObjects
{
    [ExcludeFromCodeCoverage]
    public class Appointment
    {
        public string Name { get; set; }
        public DateTimeOffset? Date { get; set; }
    }

}

using System;
using System.Diagnostics.CodeAnalysis;

namespace DataFilters.TestObjects
{
    [ExcludeFromCodeCoverage]
    public class Person
    {
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string Nickname { get; set; }

        public DateTime BirthDate { get; set; }

        public string BattleCry { get; set; }

        public int Height { get; set; }

        public DateTimeOffset DateTimeWithOffset { get; set; }
    }
}

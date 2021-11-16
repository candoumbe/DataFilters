namespace DataFilters.TestObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class Person
    {
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string Nickname { get; set; }

#if NET6_0_OR_GREATER
        public DateOnly BirthDate { get; init; }
#else
        public DateTime BirthDate { get; set; }
#endif

        public string BattleCry { get; set; }

        public int Height { get; set; }

        public DateTimeOffset DateTimeWithOffset { get; set; }
    }
}

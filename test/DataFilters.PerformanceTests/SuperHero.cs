namespace DataFilters.PerfomanceTests
{
    using System.Collections.Generic;

    internal class SuperHero
    {
        public string Nickname { get; set; }

        public string[] Powers { get; set; }

        public IEnumerable<SuperHero> Acolytes { get; set; }

    }
}

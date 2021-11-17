namespace DataFilters.TestObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [ExcludeFromCodeCoverage]
    public class SuperHero : Person
    {
#if NET6_0_OR_GREATER
        public DateOnly? LastBattleDate { get; init; }
#else
        public DateTimeOffset? LastBattleDate { get; set; }
#endif
        public Henchman Henchman { get; set; }

        public IEnumerable<string> Powers { get; set; } = Enumerable.Empty<string>();

        public int Age { get; set; }

        public IEnumerable<SuperHero> Acolytes { get; set; } = Enumerable.Empty<SuperHero>();

        public IEnumerable<Weapon> Weapons { get; set; } = Enumerable.Empty<Weapon>();
    }
}

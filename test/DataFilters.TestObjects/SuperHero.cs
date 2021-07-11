namespace DataFilters.TestObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SuperHero : Person
    {
        public DateTimeOffset? LastBattleDate { get; set; }

        public Henchman Henchman { get; set; }

        public IEnumerable<string> Powers { get; set; } = Enumerable.Empty<string>();

        public int Age { get; set; }

        public IEnumerable<SuperHero> Acolytes { get; set; } = Enumerable.Empty<SuperHero>();

        public IEnumerable<Weapon> Weapons { get; set; } = Enumerable.Empty<Weapon>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace DataFilters.TestObjects
{
    public class SuperHero : Person
    {
        public string Nickname { get; set; }

        public int Height { get; set; }

        public DateTimeOffset? LastBattleDate { get; set; }

        public Henchman Henchman { get; set; }

        public IEnumerable<string> Powers { get; set; } = Enumerable.Empty<string>();

        public int Age { get; set; }

        public IEnumerable<SuperHero> Acolytes { get; set; } = Enumerable.Empty<SuperHero>();

        public IEnumerable<Weapon> Weapons { get; set; } = Enumerable.Empty<Weapon>();
    }
}

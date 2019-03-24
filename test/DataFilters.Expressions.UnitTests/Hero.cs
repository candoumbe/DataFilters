using System;
using System.Collections.Generic;

namespace DataFilters.Expressions.UnitTests
{
    public class Hero : IEquatable<Hero>
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public override bool Equals(object obj) => Equals(obj as Hero);
        public bool Equals(Hero other) => other != null && Name == other.Name && Age == other.Age;
        public override int GetHashCode() => HashCode.Combine(Name, Age);

        public static bool operator ==(Hero hero1, Hero hero2)
        {
            return EqualityComparer<Hero>.Default.Equals(hero1, hero2);
        }

        public static bool operator !=(Hero hero1, Hero hero2)
        {
            return !(hero1 == hero2);
        }
    }
}

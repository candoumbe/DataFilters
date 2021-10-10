namespace DataFilters.Expressions.UnitTests
{
    using System;
    using System.Collections.Generic;

    public class Hero : IEquatable<Hero>
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public DateTimeOffset FirstAppearance { get; set; }

        public Hero Acolyte {get; set; }

        public override bool Equals(object obj) => Equals(obj as Hero);

        public bool Equals(Hero other) => other != null && Name == other.Name && Age == other.Age;

#if NETCOREAPP1_0 || NETCOREAPP2_0
        public override int GetHashCode() => (Name, Age).GetHashCode();
#else
        public override int GetHashCode() => HashCode.Combine(Name, Age);
#endif

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

namespace DataFilters.PerfomanceTests;

using System.Collections.Generic;

public class SuperHero
{
    public string Nickname { get; set; }

    public string[] Powers { get; set; }

    public IEnumerable<SuperHero> Acolytes { get; set; }

}
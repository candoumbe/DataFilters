namespace DataFilters.PerfomanceTests
{
    using BenchmarkDotNet.Attributes;

    using System;
    
    public class BracketVsOr
    {   
        [Benchmark]
        [Arguments("Nickname=Br[a-f]")]
        public IFilter Bracket(string input) => input.ToFilter<SuperHero>();

        [Benchmark]
        [Arguments("Nickname=Bra|Brb|Brc|Brd|Bre|Brf")]
        public IFilter Or(string input) => input.ToFilter<SuperHero>();
    }
}

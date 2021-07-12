namespace DataFilters.PerfomanceTests
{
    using BenchmarkDotNet.Attributes;

    using System;
    
    public class OneOfVsOr
    {   
        [Benchmark]
        [Arguments("Name=Br[a-f]")]
        public IFilter OneOf(string input) => input.ToFilter<SuperHero>();

        [Benchmark]
        [Arguments("Name=Bra|Brb|Brc|Brd|Bre|Brf")]
        public IFilter Or(string input) => input.ToFilter<SuperHero>();
    }
}

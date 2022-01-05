namespace DataFilters.PerfomanceTests
{
    using BenchmarkDotNet.Attributes;

    using System;
    
    [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net60)]
    [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net50)]
    [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.NetCoreApp31)]
    [MemoryDiagnoser]
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

namespace DataFilters.PerfomanceTests
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Jobs;

    using System;
    
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.CoreRt31)]
    [SimpleJob(RuntimeMoniker.Net50)]
    [SimpleJob(RuntimeMoniker.Net60)]
    public class BracketVsOr
    {   
        [Benchmark]
        [Arguments("Nickname=Br[a-f]")]
        public IFilter Bracket(string input) => input.ToFilter<SuperHero>();

        [Benchmark]
        [Arguments("Nickname=(Bra|Brb)|(Brc|Brd)|(Bre|Brf)")]
        public IFilter Or(string input) => input.ToFilter<SuperHero>();

        [Benchmark]
        [Arguments("Nickname={Bra|Brb|Brc|Brd|Bre|Brf}")]
        public IFilter OneOf(string input) => input.ToFilter<SuperHero>();
    }
}

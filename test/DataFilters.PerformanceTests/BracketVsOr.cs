namespace DataFilters.PerfomanceTests
{
    using System;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Jobs;

    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.CoreRt31)]
    [SimpleJob(RuntimeMoniker.Net50)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Marquer les membres comme étant static")]
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

namespace DataFilters.PerformanceTests
{
    using System;
    using BenchmarkDotNet.Attributes;

    [MemoryDiagnoser]
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
namespace DataFilters.PerfomanceTests;

using BenchmarkDotNet.Attributes;

using System;

[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net60)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net50)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.NetCoreApp31)]
[MemoryDiagnoser]
public class RawFilterVsFilterService
{
    private IFilterService _service;

    [Params("Nickname=Bat*")]
    public string Input { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _service = new FilterService(new FilterServiceOptions { MaxCacheSize = 10 });
    }

    [Benchmark]
    public IFilter WithoutCache() => Input.ToFilter<SuperHero>();

    [Benchmark]
    public IFilter WithCache() => _service.Compute<SuperHero>(Input);
}
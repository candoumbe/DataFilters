namespace DataFilters.PerfomanceTests;

using BenchmarkDotNet.Attributes;

using System;

[MemoryDiagnoser]
[RPlotExporter]
public class RawFilterVsFilterService
{
    private IFilterService _service;

    [Params("Nickname=(Bat|Sup|Wonder)*m[ae]n")]
    public string Input { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _service = new FilterService(new FilterServiceOptions { MaxCacheSize = 10 });
    }

    [Benchmark(Baseline = true, Description = "Computing filter with no caching")]
    public IFilter WithoutCache() => Input.ToFilter<SuperHero>();

    [Benchmark(Description = "Computing filter with a filter cache")]
    public IFilter WithCache() => _service.Compute<SuperHero>(Input);
}
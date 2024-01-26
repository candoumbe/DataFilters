namespace DataFilters.PerfomanceTests;

using System;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
[RPlotExporter]
public class RawFilterVsFilterService
{
    private IFilterService _service;

    [Params("Nickname=((Bat|Sup)|Wonder)*m[ae]n")]
    public string Input { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _service = new FilterService(new FilterServiceOptions { MaxCacheSize = 10 });
    }

    [Benchmark(Description = "Computing filter with no caching")]
    public IFilter WithoutCache() => Input.ToFilter<SuperHero>();

    [Benchmark(Description = "Computing filter with a filter cache")]
    public IFilter WithCache() => _service.Compute<SuperHero>(Input);

    [Benchmark(Baseline = true)]
    public IFilter DirectExpression()
        => new MultiFilter
        {
            Logic = FilterLogic.And,
            Filters = new[]
            {
                new MultiFilter
                {
                    Logic = FilterLogic.Or,
                    Filters = new IFilter[]
                    {
                        new MultiFilter
                        {
                            Logic = FilterLogic.Or,
                            Filters = new IFilter[]
                            {
                                new Filter(nameof(SuperHero.Nickname), FilterOperator.StartsWith, "Bat"),
                                new Filter(nameof(SuperHero.Nickname), FilterOperator.StartsWith, "Sup")
                            }
                        },
                        new Filter(nameof(SuperHero.Nickname), FilterOperator.StartsWith, "Wonder")
                    }
                },
                new MultiFilter
                {
                    Logic = FilterLogic.Or,
                    Filters = new[]
                    {
                        new Filter(nameof(SuperHero.Nickname), FilterOperator.EndsWith, "man"),
                        new Filter(nameof(SuperHero.Nickname), FilterOperator.EndsWith, "men")
                    }
                }
            }
        };
}
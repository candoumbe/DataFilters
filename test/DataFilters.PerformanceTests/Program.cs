using BenchmarkDotNet.Running;

namespace DataFilters.PerformanceTests;

public class Program
{
    static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
        .Run(args);
}
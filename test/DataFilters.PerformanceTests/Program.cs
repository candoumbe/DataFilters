using System;

namespace DataFilters.PerfomanceTests
{
    using BenchmarkDotNet.Running;

    public class Program
    {
        static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                                                            .Run(args);
    }
}

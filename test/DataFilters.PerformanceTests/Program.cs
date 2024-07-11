using BenchmarkDotNet.Running;
namespace DataFilters.PerfomanceTests
{

    public class Program
    {
        static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                                                            .Run(args);
    }
}

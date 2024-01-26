using System;
using FsCheck;
using FsCheck.Fluent;

namespace DataFilters.UnitTests.Helpers;

internal static class GeneratorHelper
{
    public static Arbitrary<T> GetArbitraryFor<T>(Func<T, bool> filter = null) => ArbMap.Default.ArbFor<T>()
                                                                                                .Filter(item => filter?.Invoke(item) != false);
}

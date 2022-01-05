#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataFilters;

/// <summary>
/// A service that can build <see cref="IFilter"/> instances
/// </summary>
public interface IFilterService
{
    /// <summary>
    /// Computes a <see cref="IFilter"/> instance.
    /// </summary>
    /// <typeparam name="T">Type onto which the resulting filter should be applied</typeparam>
    /// <param name="input">Query string that describe the filter to apply</param>
    /// <returns>an <see cref="IFilter"/> instance</returns>
    IFilter Compute<T>(string input);
}

#endif
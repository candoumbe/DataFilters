#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER

using DataFilters.Casing;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataFilters;

/// <summary>
/// <para>
/// <see cref="IFilterService"/> implementation with a local L.R.U cache.
/// This service can be used wherever you need to build an <see cref="IFilter"/> instance for a given input.
/// </para>
/// <para>
/// <example> Replace the manual mapping of an input to <see cref="IFilter"/>
/// string input = "Nickname=*man&amp;Town=Gotham"
/// (PropertyName propName, FilterExpression)[] parseResults = FilterTokenParser.Parse(input)
/// IList&lt;IFilter&gt; filters = new List&lt;IFilter&gt;(parseResults.Length);
/// </example>
/// </para>
/// </summary>
public class FilterService : IFilterService
{
    private readonly FilterServiceOptions _options;
    private readonly ConcurrentDictionary<string, (long Timestamp, IFilter Filter)> _cache;

    /// <summary>
    /// Builds a new <see cref="FilterService"/>
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c>.</exception>
    public FilterService(FilterServiceOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        _options = options;
        _cache = new();
    }

    ///<inheritdoc/>
    public IFilter Compute<T>(string input)
    {
        string key = $"{typeof(T).FullName}_{input}";
        IFilter filter;
        if (!_cache.TryGetValue(key, out (long TimeStamp, IFilter Filter) value))
        {
            if ( _cache.Count == _options.MaxCacheSize)
            {
#if NET6_0_OR_GREATER
                string oldestKey = _cache.MinBy(entry => entry.Value.Timestamp).Key;
#else
                string oldestKey = _cache.OrderBy(entry => entry.Value.Timestamp).First().Key;
#endif

                _cache.TryRemove(oldestKey, out _);
            }
            filter = input.ToFilter<T>(_options.PropertyNameResolutionStrategy);
            _cache.TryAdd(key, (DateTime.UtcNow.Ticks, filter));
        }
        else
        {
            _cache.TryUpdate(key, (DateTime.UtcNow.Ticks, value.Filter), value);
            filter = value.Filter;
        }

        return filter;
    }
}
#endif
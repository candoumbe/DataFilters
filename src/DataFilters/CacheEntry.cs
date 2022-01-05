#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER

namespace DataFilters;
#if NET5_0_OR_GREATER
internal record CacheEntry<TValue>(long Timestamp, TValue Value);
#else
internal class CacheEntry<TValue>
{
    public TValue Value { get; }

    public long Timestamp { get; }

    internal CacheEntry(TValue value, long timestamp)
    {
        Value = value;
        Timestamp = timestamp;
    }
}
#endif

#endif
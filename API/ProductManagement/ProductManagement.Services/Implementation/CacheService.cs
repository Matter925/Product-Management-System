using System.Collections;
using System.Reflection;
using System.Text.Json;

using Microsoft.Extensions.Caching.Memory;

using ProductManagement.Services.Interfaces;

namespace ProductManagement.Services.Implementation;
public class CacheService(IMemoryCache cache) : ICacheService
{
    private readonly HashSet<string> _cacheKeys = [];
    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Stores a serialized response in the cache with the specified cache key and time to live.
    /// </summary>
    /// <param name="cacheKey">The unique key associated with the cached response.</param>
    /// <param name="response">The object to be cached. It must be serializable to JSON.</param>
    /// <param name="timeToLive">The duration for which the cached response should remain valid.</param>
    public void SetCacheResponse(string cacheKey, object? response, TimeSpan timeToLive)
    {
        if (response == null || timeToLive <= TimeSpan.Zero)
            return;

        var serializedResponse = JsonSerializer.Serialize(response, _serializerOptions);

        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = timeToLive,
            Priority = CacheItemPriority.Normal
        }.RegisterPostEvictionCallback((key, value, reason, state) =>
        {
            _cacheKeys.Remove(key.ToString() ?? string.Empty);
        });

        cache.Set(cacheKey, serializedResponse, cacheEntryOptions);
        _cacheKeys.Add(cacheKey);
    }

    /// <summary>
    /// Retrieves a cached response from the cache using the specified cache key.
    /// </summary>
    /// <param name="cacheKey">The unique key associated with the cached response.</param>
    /// <returns>
    /// The serialized response as a JSON string if found in the cache; otherwise, returns null.
    /// </returns>
    public string? GetCachedResponse(string cacheKey)
    {
        return cache.TryGetValue(cacheKey, out string? cachedData) ? cachedData : null;
    }

    /// <summary>
    /// Removes the cached response associated with the specified cache key.
    /// </summary>
    /// <param name="cacheKey">The unique key associated with the cached response to be removed.</param>
    public void RemoveCachedResponse(string cacheKey)
    {
        cache.Remove(cacheKey);
        _cacheKeys.Remove(cacheKey);
    }

    /// <summary>
    /// Retrieves the cache keys currently stored in the memory cache.
    /// </summary>
    /// <returns>An IEnumerable of strings representing the cache keys.</returns>
    public IEnumerable<string> GetCacheKeys()
    {
        return [.. _cacheKeys];
    }
}

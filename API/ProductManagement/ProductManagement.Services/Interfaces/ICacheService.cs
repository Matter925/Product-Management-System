namespace ProductManagement.Services.Interfaces;

public interface ICacheService
{
    void SetCacheResponse(string cacheKey, object? response, TimeSpan timeToLive);
    string? GetCachedResponse(string cacheKey);
    void RemoveCachedResponse(string cacheKey);
    public IEnumerable<string> GetCacheKeys();
}

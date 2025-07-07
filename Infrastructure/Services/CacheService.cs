using Microsoft.Extensions.Caching.Hybrid;

namespace ExpenseTrackerApi.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly HybridCache _hybridCache;

    public CacheService(HybridCache hybridCache)
    {
        _hybridCache = hybridCache;
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null)
    {
        var options = new HybridCacheEntryOptions
        {
            Expiration = absoluteExpiration ?? TimeSpan.FromMinutes(60)
        };
        return await _hybridCache.GetOrCreateAsync(key, async token => await factory(), options, cancellationToken: default);
    }

    public async Task RemoveAsync(string key)
    {
        await _hybridCache.RemoveAsync(key);
    }
}
namespace ExpenseTrackerApi.Infrastructure.Services;

public interface ICacheService
{
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null);
    Task RemoveAsync(string key);
}
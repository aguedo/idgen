
using System.Collections.Concurrent;

namespace Aslanta.Idgen.Api;

public class IdCache
{
    // We can adjust this value to change the number of IDs we want to cache.
    // Note that the Ids in the cache are lost when the application is restarted
    // since they were already deleted from the database.
    private const int CacheSize = 200;
    private readonly ICacheRepository _cacheRepository;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ConcurrentBag<string> _shortIds = new();

    public IdCache(ICacheRepository cacheRepository)
    {
        _cacheRepository = cacheRepository;
    }

    public async ValueTask<string> GetId()
    {
        string? id;
        while (!_shortIds.TryTake(out id))
        {
            await ReloadCacheAsync().ConfigureAwait(false);
        }

        return id;
    }

    private async Task ReloadCacheAsync()
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_shortIds.Count > 0)
            {
                return;
            }

            List<string> ids = await _cacheRepository.GetIds(CacheSize).ConfigureAwait(false);
            foreach (string id in ids)
            {
                _shortIds.Add(id);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

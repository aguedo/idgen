using System.Collections.Concurrent;


namespace Aslanta.Idgen.Api;

public interface IIdgenService
{
    Task<string> GetId();
}

public class IdgenService : IIdgenService
{
    private IdCache _idCache;

    public IdgenService(IdCache idCache)
    {
        _idCache = idCache;
    }

    public async Task<string> GetId()
    {
        return await _idCache.GetId();
    }
}

public class IdCache
{
    private ConcurrentBag<string> _ids = new ConcurrentBag<string>();

    public async ValueTask<string> GetId()
    {
        string? id;
        while (!_ids.TryTake(out id))
        {
            await ReloadCacheAsync().ConfigureAwait(false);
        }

        return id;
    }

    private async Task ReloadCacheAsync()
    {
        // TODO:
        // Load and delete the previously generated ids in database
        for (int i = 0; i < 10; i++)
        {
            // Generate a short ID by taking the first 7 characters of a GUID
            string shortId = Guid.NewGuid().ToString("N").Substring(0, 7);
            _ids.Add(shortId);
        }

        await Task.CompletedTask; // TODO: Implement
    }
}

using System.Collections.Concurrent;
using System.Data.Common;
using System.Security.Cryptography;
using Npgsql;

public class IdCache
{
    private readonly ReaderWriterLockSlim _readerWriterLock = new();
    private readonly ConcurrentBag<string> _shortIds = new();

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
        _readerWriterLock.EnterWriteLock();
        try
        {
            if (_shortIds.Count > 0)
            {
                return;
            }

            const int cacheSize = 10; // TODO: Change to 200.
            for (int i = 0; i < cacheSize; i++)
            {
                _shortIds.Add(ShortIdGenerator.GenerateId());
            }

            await Task.CompletedTask;

            // string connectionString = ""; // TODO: Add connection string.
            // using var connection = new NpgsqlConnection(connectionString);
            // connection.Open();

            // string sql = $@"
            // DELETE FROM ShortIds
            // WHERE Id IN (
            //     SELECT Id FROM ShortIds
            //     ORDER BY Id
            //     LIMIT 200
            // )
            // RETURNING ShortId;";

            // using var cmd = new NpgsqlCommand(sql, connection);
            // using DbDataReader reader = await cmd.ExecuteReaderAsync();
            // while (await reader.ReadAsync())
            // {
            //     _shortIds.Add(reader.GetString(0));
            // }
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
    }

    // TODO: delete this method when the SQL code is uncommented.
    static class ShortIdGenerator
    {
        private static readonly char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
        private const int IdLength = 6;

        public static string GenerateId()
        {
            char[] id = new char[IdLength];
            for (int i = 0; i < IdLength; i++)
            {
                int index = RandomNumberGenerator.GetInt32(0, chars.Length);
                id[i] = chars[index];
            }
            return new string(id);
        }
    }
}
using System.Data.Common;
using Npgsql;

namespace Aslanta.Idgen.Api;

public class CacheRepository : ICacheRepository
{
    public async Task<List<string>> GetIds(int count)
    {
        using var connection = new NpgsqlConnection(Config.ConnectionString);
        connection.Open();

        string sql = $@"
        DELETE FROM ShortIds
        WHERE Id IN (
            SELECT Id FROM ShortIds
            ORDER BY Id
            LIMIT {count}
        )
        RETURNING ShortId;";

        var ids = new List<string>(count);
        using var cmd = new NpgsqlCommand(sql, connection);
        using DbDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ids.Add(reader.GetString(0));
        }

        return ids;
    }
}
using System.Text;
using Npgsql;

namespace Aslanta.Idgen.Job;

internal static class ShortIdService
{
    private const int BatchCount = 500;
    private const int NewIdsCount = 100000;
    private const int MinIdCount = 10000;

    public static void GenerateId()
    {
        int availableIdCount = AvailableIdCount();
        if (availableIdCount > MinIdCount)
        {
            Console.WriteLine($"There are {availableIdCount} ids already in the database.");
            return;
        }

        int rowsCount = NewIdsCount;
        while (rowsCount > 0)
        {
            HashSet<string> batch = GenerateIds(BatchCount);
            int savedIds = SaveIds(batch);
            rowsCount -= batch.Count;
            Console.WriteLine($"Saved {savedIds} ids. Remaining: {rowsCount}");
        }
    }

    private static int SaveIds(IEnumerable<string> shortIdsBatch)
    {
        try
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO ShortIds (ShortId) VALUES ");
            var parameters = new List<NpgsqlParameter>();

            int i = 0;
            foreach (string shortId in shortIdsBatch)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append($"(@p{i})");
                parameters.Add(new NpgsqlParameter($"@p{i}", shortId));
                i++;
            }

            string sql = sb.ToString();
            using var connection = new NpgsqlConnection(Config.ConnectionString);
            connection.Open();

            using NpgsqlTransaction transaction = connection.BeginTransaction();
            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddRange(parameters.ToArray());

            cmd.ExecuteNonQuery();
            transaction.Commit();
            return i;
        }
        catch
        {
            // TODO: Log the exception
            return 0;
        }
    }

    private static HashSet<string> GenerateIds(int count)
    {
        try
        {
            HashSet<string> memoryIds = GenerateIdsInMemory(count);
            // Make sure that the generated ids are unique by trying to add them to the history database table.
            return AvailableIds(memoryIds);
        }
        catch
        {
            // TODO: Log the exception
            return [];
        }
    }

    private static HashSet<string> AvailableIds(HashSet<string> memoryIds)
    {
        using var connection = new NpgsqlConnection(Config.ConnectionString);
        connection.Open();

        using NpgsqlTransaction transaction = connection.BeginTransaction();
        var availableIds = new HashSet<string>();

        var cmd = new NpgsqlCommand("INSERT INTO ShortIdsHistory (ShortId) VALUES (@shortId) ON CONFLICT DO NOTHING RETURNING ShortId", connection, transaction);
        cmd.Parameters.Add(new NpgsqlParameter("shortId", NpgsqlTypes.NpgsqlDbType.Varchar));
        cmd.Prepare();

        // If this is too slow, consider using a batch insert
        foreach (string shortId in memoryIds)
        {
            cmd.Parameters["shortId"].Value = shortId;
            using NpgsqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                availableIds.Add(reader.GetString(0));
            }
        }

        transaction.Commit();
        return availableIds;
    }

    private static int AvailableIdCount()
    {
        using var connection = new NpgsqlConnection(Config.ConnectionString);
        connection.Open();
        using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM ShortIds", connection);
        using NpgsqlDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? reader.GetInt32(0) : 0;
    }

    private static HashSet<string> GenerateIdsInMemory(int count)
    {
        HashSet<string> ids = new();
        while (ids.Count < count)
        {
            ids.Add(ShortIdGenerator.GenerateId());
        }

        return ids;
    }
}

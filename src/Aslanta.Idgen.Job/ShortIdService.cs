using System.Text;
using Npgsql;

namespace Aslanta.Idgen.Job;

internal static class ShortIdService
{
    public static void GenerateId()
    {
        const int batchCount = 500;
        int rowsCount = 100000;

        while (rowsCount > 0)
        {
            HashSet<string> batch = GenerateIds(batchCount);
            if (batch.Count == rowsCount)
            {
                continue;
            }

            int savedIds = SaveIds(batch);
            rowsCount -= batch.Count;
            // TODO: log progress
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
            string connectionString = ""; // TODO: Add connection string
            using var connection = new NpgsqlConnection(connectionString);
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
        string connectionString = ""; // TODO: Add connection string
        using var connection = new NpgsqlConnection(connectionString);
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

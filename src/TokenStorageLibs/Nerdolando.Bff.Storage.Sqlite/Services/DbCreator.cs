using System.Data.Common;

namespace Nerdolando.Bff.Storage.Sqlite.Services
{
    internal class DbCreator(SqliteConnectionFactory _sqliteConnectionFactory)
    {
        private const int RequiredVersion = 1;
        public async Task CreateDatabaseAsync(CancellationToken ct = default)
        {
            using var connection = await _sqliteConnectionFactory.CreateConnection().ConfigureAwait(false);

            var currentVersion = await GetDbVersion(connection, ct).ConfigureAwait(false);

            if (currentVersion != RequiredVersion)
            {
                await DropSchema(connection, ct).ConfigureAwait(false);
                await CreateDbSchema(connection, ct).ConfigureAwait(false);
                await SetDbVersion(connection, RequiredVersion, ct).ConfigureAwait(false);

                await ExecAsync(connection, "PRAGMA journal_mode=WAL;", ct);
                await ExecAsync(connection, "PRAGMA foreign_keys=ON;", ct);
                await ExecAsync(connection, "PRAGMA synchronous=NORMAL;", ct);
            }
        }

        private static async Task<int> GetDbVersion(DbConnection connection, CancellationToken ct)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "PRAGMA user_version;";
            var result = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt32(result);
        }

        private static async Task SetDbVersion(DbConnection connection, int version, CancellationToken ct)
        {
            await ExecAsync(connection, $"PRAGMA user_version = {version};", ct).ConfigureAwait(false);
        }

        private static async Task CreateDbSchema(DbConnection connection, CancellationToken ct)
        {
            var createCommand = 
                """
                    CREATE TABLE IF NOT EXISTS tokens (
                        session_id TEXT PRIMARY KEY UNIQUE,
                        access_token TEXT NOT NULL,
                        refresh_token TEXT NOT NULL,
                        expires_at TEXT NOT NULL
                );
                """;            
            
            await ExecAsync(connection, createCommand, ct).ConfigureAwait(false);
        }

        private static async Task DropSchema(DbConnection connection, CancellationToken ct)
        {
            var dropCommand = 
                """
                    DROP TABLE IF EXISTS tokens;
                """;
            await ExecAsync(connection, dropCommand, ct).ConfigureAwait(false);
        }

        private static async Task ExecAsync(DbConnection cn, string sql, CancellationToken ct)
        {
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}

using Nerdolando.Bff.Abstractions;
using System.Data;
using System.Data.Common;

namespace Nerdolando.Bff.Storage.Sqlite.Services
{
    internal class SQLiteUserTokenStorage(SqliteConnectionFactory _sqliteConnectionFactory) : IUserTokenStorage
    {
        public async Task<UserToken?> GetTokenAsync(Guid sessionId, CancellationToken ct = default)
        {
            using var connection = await _sqliteConnectionFactory.CreateConnection().ConfigureAwait(false);

            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT session_id, access_token, refresh_token, expires_at FROM tokens WHERE session_id = @sid LIMIT 1";
                AddParameter(cmd, "@sid", sessionId.ToString());

                using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                    return null;

                var token = new UserToken
                {
                    SessionId = Guid.Parse(reader.GetString("session_id")),
                    AccessToken = reader.GetString("access_token"),
                    RefreshToken = reader.GetString("refresh_token"),
                    ExpiresAt = DateTimeOffset.Parse(reader.GetString("expires_at"), System.Globalization.CultureInfo.InvariantCulture)
                };
                return token;
            }
            catch
            {
                return null;
            }
        }

        public async Task StoreTokenAsync(UserToken token, CancellationToken ct = default)
        {
            using var connection = await _sqliteConnectionFactory.CreateConnection().ConfigureAwait(false);

            // Upsert (SQLite syntax)
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                """
                    INSERT INTO tokens (session_id, access_token, refresh_token, expires_at)
                    VALUES (@sid, @acc, @ref, @exp)
                    ON CONFLICT(session_id) DO UPDATE SET
                      access_token = excluded.access_token,
                      refresh_token = excluded.refresh_token,
                      expires_at = excluded.expires_at;
                """;
            AddParameter(cmd, "@sid", token.SessionId.ToString());
            AddParameter(cmd, "@acc", token.AccessToken);
            AddParameter(cmd, "@ref", token.RefreshToken);
            AddParameter(cmd, "@exp", token.ExpiresAt.ToString("O")); // ISO 8601

            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        private static void AddParameter(DbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
    }
}

using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace Nerdolando.Bff.Storage.Sqlite
{
    /// <summary>
    /// Holds information needed to open a SQLite database and can create an ADO.NET DbConnection
    /// without taking a compile-time dependency on a specific provider. At runtime either
    /// Microsoft.Data.Sqlite or System.Data.SQLite must be available.
    /// </summary>
    internal sealed class SqliteConnectionFactory
    {
        public string FilePath { get; }
        public string ConnectionString { get; }

        public SqliteConnectionFactory(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path required", nameof(filePath));

            FilePath = Path.GetFullPath(filePath);

            ConnectionString = $"Data Source={FilePath};Cache=Shared;Mode=ReadWriteCreate";
        }

        /// <summary>
        /// Creates a new unopened DbConnection for the configured database.
        /// Tries Microsoft.Data.Sqlite first, then System.Data.SQLite. Throws if neither is loaded.
        /// </summary>
        public async Task<DbConnection> CreateConnection(CancellationToken ct = default)
        {
            var connection = new SqliteConnection(ConnectionString);
            SQLitePCL.Batteries.Init();

            await connection.OpenAsync(ct).ConfigureAwait(false);
            return connection;
        }
    }
}

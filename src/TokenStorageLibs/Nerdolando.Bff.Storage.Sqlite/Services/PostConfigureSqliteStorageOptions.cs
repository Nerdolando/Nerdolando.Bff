using Microsoft.Extensions.Options;

namespace Nerdolando.Bff.Storage.Sqlite.Services
{
    internal sealed class PostConfigureSqliteStorageOptions(DbCreator _dbCreator) : IPostConfigureOptions<SqliteStorageOptions>
    {
        public void PostConfigure(string? name, SqliteStorageOptions options)
        {
            _dbCreator.CreateDatabaseAsync().GetAwaiter().GetResult();
        }
    }

}

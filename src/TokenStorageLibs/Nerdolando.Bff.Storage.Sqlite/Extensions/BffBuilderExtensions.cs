using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.Storage.Sqlite.Services;

namespace Nerdolando.Bff.Storage.Sqlite.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class BffBuilderExtensions
    {
        /// <summary>
        /// Uses SQLite for token storage with default options.
        /// You DO NOT need to have SQLite support in your application. This package includes everything needed to use SQLite.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IBffBuilder UseSqliteTokenStorage(this IBffBuilder builder)
        {
            return builder.UseSqliteTokenStorage(options => { });
        }

        /// <summary>
        /// Uses SQLite for token storage with custom options.
        /// You DO NOT need to have SQLite support in your application. This package includes everything needed to use SQLite.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">Action to configure Sqlite options</param>
        /// <returns></returns>
        public static IBffBuilder UseSqliteTokenStorage(this IBffBuilder builder, Action<SqliteStorageOptions> options)
        {
            builder.Services.AddOptions<SqliteStorageOptions>()
                .Configure(options)
                .ValidateOnStart();

            var sqliteOptions = new SqliteStorageOptions();
            options(sqliteOptions);

            builder.Services.AddSingleton<DbCreator>();

            builder.Services.AddSingleton(sp =>
            {
                return new SqliteConnectionFactory(sqliteOptions.FilePath);
            });

            builder.Services.AddSingleton<IPostConfigureOptions<SqliteStorageOptions>, PostConfigureSqliteStorageOptions>();
            return builder.UseCustomTokenStorage<SQLiteUserTokenStorage>();
        }
    }
}

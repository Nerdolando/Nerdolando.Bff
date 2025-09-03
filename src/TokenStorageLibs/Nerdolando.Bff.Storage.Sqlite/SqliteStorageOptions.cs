namespace Nerdolando.Bff.Storage.Sqlite
{
    /// <summary>
    /// Options for configuring SQLite storage for tokens.
    /// </summary>
    public class SqliteStorageOptions
    {
        /// <summary>
        /// Specifies the file path for the SQLite database.
        /// </summary>
        public string FilePath { get; set; } = "bff_storage.db";
    }
}

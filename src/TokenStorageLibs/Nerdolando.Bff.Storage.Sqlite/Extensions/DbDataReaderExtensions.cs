using System.Data.Common;

namespace Nerdolando.Bff.Storage.Sqlite.Extensions
{
    internal static class DbDataReaderExtensions
    {
        public static string GetStringOrEmpty(this DbDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }
    }
}

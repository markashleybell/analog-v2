using Dapper;
using DuckDB.NET.Data;
using static analog.Data;

namespace analog
{
    public static class DuckDb
    {
        public static string DbFilePath =>
            @"D:\Src\temp\duckdb_logs.db";

        public static string InsertConnectionString =>
            $"Data Source={DbFilePath}";

        public static string ReadConnectionString =>
            $"Data Source={DbFilePath};ACCESS_MODE=READ_ONLY";

        public static string InMemoryConnectionString =>
            $"Data Source={DuckDBConnectionStringBuilder.InMemorySharedDataSource}";

        public static string GenerateTableSql(IEnumerable<DatabaseColumn> columns)
        {
            static string asColumnDefinition(DatabaseColumn c) =>
                $"{c.Name} {c.DataType}";

            var columnDefinitions = string.Join("," + Environment.NewLine + "    ", columns.Select(asColumnDefinition));

            return $@"CREATE TABLE entries (
    {columnDefinitions}
)";
        }

        public static readonly ColumnMapping[] DefaultIISW3CLogMappings = [
            new ColumnMapping(["date", "time"], "date", "TIMESTAMP", DefaultIISW3CTimestampGetterFactory),
        new ColumnMapping(["s-ip"], "serverip", "VARCHAR"),
        new ColumnMapping(["cs-method"], "method", "VARCHAR"),
        new ColumnMapping(["cs-uri-stem"], "url", "VARCHAR"),
        new ColumnMapping(["cs-uri-query"], "querystring", "VARCHAR"),
        new ColumnMapping(["cs-username"], "username", "VARCHAR"),
        new ColumnMapping(["c-ip", "cf-client-ip"], "ip", "VARCHAR", CloudflareClientIpAddressGetterFactory),
        new ColumnMapping(["cs(User-Agent)"], "useragent", "VARCHAR"),
        new ColumnMapping(["cs(Referer)"], "referer", "VARCHAR"),
        new ColumnMapping(["s-port"], "port", "INTEGER"),
        new ColumnMapping(["sc-status"], "status", "SMALLINT"),
        new ColumnMapping(["sc-substatus"], "substatus", "SMALLINT"),
        new ColumnMapping(["sc-win32-status"], "winstatus", "SMALLINT"),
        new ColumnMapping(["time-taken"], "duration", "INTEGER"),
        new ColumnMapping(["sc-bytes"], "bytessent", "INTEGER"),
        new ColumnMapping(["cs-bytes"], "bytesrecieved", "INTEGER"),
        new ColumnMapping(["is-facebook-asn"], "facebookasn", "VARCHAR")
        ];

        public static int PopulateDatabaseFromFiles(
            string connectionString,
            IEnumerable<string> files,
            IEnumerable<DatabaseColumn> databaseColumns)
        {
            if (File.Exists(DbFilePath))
            {
                File.Delete(DbFilePath);
            }

            var resultCount = 0;

            using var connection = new DuckDBConnection(connectionString);

            connection.Open();

            connection.Execute(GenerateTableSql(databaseColumns));

            using var appender = connection.CreateAppender("entries");

            foreach (var log in files)
            {
                foreach (var entry in GetLogEntries(log))
                {
                    var row = appender.CreateRow();

                    foreach (var c in databaseColumns)
                    {
                        row.AppendValue(c.GetValue(entry));
                    }

                    row.EndRow();

                    resultCount++;
                }
            }

            return resultCount;
        }
    }
}

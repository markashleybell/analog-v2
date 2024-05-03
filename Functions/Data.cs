namespace analog
{
    public static class Data
    {
        public static readonly Func<IEnumerable<LogColumn>, Func<string[], string>> DefaultIISW3CTimestampGetterFactory =
            columns =>
            {
                var date = columns.SingleOrDefault(c => c.Name == "date");
                var time = columns.SingleOrDefault(c => c.Name == "time");

                if (date is null || time is null)
                {
                    throw new Exception("Default timestamp columns 'date' and 'time' not found in source log");
                }

                return cols => cols[date.Index] + " " + cols[time.Index];
            };

        public static readonly Func<IEnumerable<LogColumn>, Func<string[], string>> CloudflareClientIpAddressGetterFactory =
            columns =>
            {
                var column = columns.SingleOrDefault(c => c.Name == "cf-client-ip")
                    ?? columns.SingleOrDefault(c => c.Name == "c-ip")
                    ?? throw new Exception("No 'cf-client-ip' or 'c-ip' column found in source log");

                return cols => cols[column.Index];
            };

        public static IEnumerable<string[]> GetLogEntries(string path)
        {
            foreach (var line in File.ReadLines(path))
            {
                if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                yield return line.Split(' ');
            }
        }

        public static IEnumerable<LogColumn> GetLogColumns(IEnumerable<string> fileHeader)
        {
            const string headerLinePrefix = "#Fields:";

            var columnNamesLine = fileHeader
                .SingleOrDefault(l => l.StartsWith(headerLinePrefix, StringComparison.OrdinalIgnoreCase));

            return columnNamesLine is not null
                ? columnNamesLine
                    .Replace(headerLinePrefix, string.Empty).Trim()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select((c, i) => new LogColumn(i, c))
                : [];
        }

        public static (bool valid, IEnumerable<LogColumn> columns, string? message) ValidateAndReturnColumns(IEnumerable<string> files)
        {
            var currentColumns = Enumerable.Empty<LogColumn>();

            foreach (var file in files)
            {
                var lines = File.ReadLines(file);

                var header = lines.Take(10);

                var columns = GetLogColumns(header);

                var columnsMatch = !currentColumns.Any() || columns.SequenceEqual(currentColumns);

                if (!columnsMatch)
                {
                    return (false, columns, $"Columns in file {file} do not match previous files.");
                }

                currentColumns = columns;
            }

            return (true, currentColumns, default);
        }

        public static (IEnumerable<DatabaseColumn> databaseColumns, IEnumerable<string> errors) GetDatabaseColumns(
            IEnumerable<LogColumn> logColumns,
            ColumnMapping[] mappings)
        {
            var databaseColumns = new List<DatabaseColumn>();
            var errors = new List<string>();

            var remainingColumns = new List<LogColumn>(logColumns).AsEnumerable();

            /*
            Note that even though we're in a for loop, we create a
            *separate* variable (idx) to increment the index for the
            output database column.

            This is because database columns can potentially contain
            the combined or transformed values from multiple source
            log columns; in this case the index values diverge, so
            we need an independent counter for the output index.
            */
            for (int i = 0, idx = 0; i < mappings.Length; i++)
            {
                var mapping = mappings[i];

                var sourceColumnsForMapping = remainingColumns
                    .Where(c => mapping.LogColumnNames.Any(lc => string.CompareOrdinal(c.Name, lc) == 0));

                if (!sourceColumnsForMapping.Any())
                {
                    /*
                    Log files vary widely even between individual IIS sites,
                    so if the column(s) specified in the mapping don't exist
                    in this log file, we just make a note and move on.
                    */

                    errors.Add($"Log column(s) '{string.Join("', '", mapping.LogColumnNames)}' not found in source log");

                    continue;
                }

                var valueGetter = mapping.CreateLogValueGetter(sourceColumnsForMapping);

                var databaseColumn = new DatabaseColumn(
                    index: idx++,
                    name: mapping.DatabaseColumnName,
                    dataType: mapping.DatabaseColumnType,
                    sources: sourceColumnsForMapping,
                    valueGetter: valueGetter);

                databaseColumns.Add(databaseColumn);

                remainingColumns = remainingColumns.Except(sourceColumnsForMapping);
            }

            // TODO: Handle columns which aren't in the mapping list

            return (databaseColumns, errors);
        }
    }
}

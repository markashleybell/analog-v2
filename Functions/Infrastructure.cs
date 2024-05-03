using System.Net.NetworkInformation;
using System.Reflection;
using DuckDB.NET.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using static analog.Data;

namespace analog;

public static class Infrastructure
{
    private const string LocalhostCorsPolicy = nameof(LocalhostCorsPolicy);

    public const string WebRootFolder = "wwwroot";
    public const string ResourcesFolder = "Resources";

    public static (WebApplication Server, string BaseUrl) CreateLocalServer(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            WebRootPath = WebRootFolder
        });

        var embeddedFileProvider = new ManifestEmbeddedFileProvider(
            Assembly.GetEntryAssembly() ?? throw new Exception("Failed to load entry point assembly"),
            $"{ResourcesFolder}/{WebRootFolder}");

        builder.Environment.WebRootFileProvider = new CompositeFileProvider(
            embeddedFileProvider,
            builder.Environment.WebRootFileProvider);

        const int startPort = 8600;
        const int endPort = 8700;

        var portsInUse = IPGlobalProperties
            .GetIPGlobalProperties()
            .GetActiveTcpListeners()
            .Where(ep => ep.Port >= startPort && ep.Port <= endPort)
            .Select(ep => ep.Port)
            .ToArray();

        var port = startPort;

        for (int i = startPort; i <= endPort; i++)
        {
            port = i;

            if (!portsInUse.Contains(port))
            {
                break;
            }

            if (port == endPort)
            {
                throw new SystemException($"Couldn't find open port within range {startPort}-{endPort}.");
            }
        }

        var baseUrl = $"http://localhost:{port}";

        builder.WebHost.UseUrls(baseUrl);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                name: LocalhostCorsPolicy,
                policy => policy
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithOrigins("http://localhost:5174"));
        });

        var app = builder.Build();

        app.UseCors(LocalhostCorsPolicy);

        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                // const int durationInSeconds = 60 * 60 * 24; // 24 hours
                // ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                //     "public,max-age=" + durationInSeconds;

                ctx.Context.Response.Headers[HeaderNames.CacheControl] = "no-cache";
            }
        });

        app.MapGet("/getfiles", (string folder) => new { files = Directory.GetFiles(folder, "*.log").Select(f => new { path = f }) });

        app.MapPost("/loaddata", (string[] files) =>
        {
            var (valid, logColumns, errorMessage) = ValidateAndReturnColumns(files);

            var (databaseColumns, errors) = GetDatabaseColumns(logColumns, DuckDb.DefaultIISW3CLogMappings);

            var inserted = DuckDb.PopulateDatabaseFromFiles(DuckDb.InsertConnectionString, files, databaseColumns);

            return new
            {
                inserted,
                databaseColumns
            };
        });

        app.MapGet("/query", (string query, int pageSize, int page) =>
        {
            var offset = pageSize * (page - 1);

            if (!query.Contains("ORDER BY"))
            {
                query += " ORDER BY date";
            }

            IEnumerable<object[]> GetResults()
            {
                using var conn = new DuckDBConnection(DuckDb.ReadConnectionString);

                conn.Open();

                var command = conn.CreateCommand();

                command.CommandText = $"FROM entries {query} LIMIT {pageSize} OFFSET {offset}";

                using var reader = command.ExecuteReader();

                yield return Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToArray();

                while (reader.Read())
                {
                    yield return Enumerable.Range(0, reader.FieldCount)
                        .Select(i =>
                        {
                            return reader.GetValue(i) switch
                            {
                                DateTime dt => dt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"),
                                int n => n.ToString(),
                                short n => n.ToString(),
                                string s => s,
                                _ => ""
                            };
                        })
                        .ToArray();
                }
            }

            return GetResults();
        });

        return (app, baseUrl);
    }
}

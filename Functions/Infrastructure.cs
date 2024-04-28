using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

namespace Photino.HelloPhotino.Vue;

public static class Infrastructure
{
    public const string WebRootFolder = "wwwroot";
    public const string ResourcesFolder = "Resources";

    public static (WebApplication Server, string BaseUrl) CreateStaticFileServer(string[] args)
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

        var app = builder.Build();

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

        return (app, baseUrl);
    }
}

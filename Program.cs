using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using PhotinoNET;
using PhotinoNET.Server;

namespace Photino.HelloPhotino.Vue;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // PhotinoServer
        //     .CreateStaticFileServer(args, out string baseUrl)
        //     .RunAsync();

        var webRootFolder = "wwwroot";
        var startPort = 8000;
        var portRange = 100;
        var baseUrl = "";

        WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            WebRootPath = webRootFolder
        });
        ManifestEmbeddedFileProvider manifestEmbeddedFileProvider = new ManifestEmbeddedFileProvider(Assembly.GetEntryAssembly(), "Resources/" + webRootFolder);
        IFileProvider webRootFileProvider = webApplicationBuilder.Environment.WebRootFileProvider;
        CompositeFileProvider webRootFileProvider2 = new CompositeFileProvider(manifestEmbeddedFileProvider, webRootFileProvider);
        webApplicationBuilder.Environment.WebRootFileProvider = webRootFileProvider2;
        int port;
        for (port = startPort; IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any((IPEndPoint x) => x.Port == port); port++)
        {
            if (port > port + portRange)
            {
                throw new SystemException($"Couldn't find open port within range {port - portRange} - {port}.");
            }
        }

        baseUrl = $"http://localhost:{port}";
        webApplicationBuilder.WebHost.UseUrls(baseUrl);
        WebApplication webApplication = webApplicationBuilder.Build();
        webApplication.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                const int durationInSeconds = 60 * 60 * 24;
                ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                    "public,max-age=" + durationInSeconds;
            }
        });

        webApplication.RunAsync();

        var window = new PhotinoWindow()
            .SetTitle("AnaLog 2.0")
            .SetUseOsDefaultSize(false)
            .SetUseOsDefaultLocation(false)
            .SetSize(new System.Drawing.Size(1920, 1440))
            .Center()
            // .SetDevToolsEnabled(false)
            .SetResizable(true)
            /*
            .RegisterCustomSchemeHandler("app", (object sender, string scheme, string url, out string contentType) =>
            {
                contentType = "text/javascript";
                return new MemoryStream(Encoding.UTF8.GetBytes(@"
                        (() =>{
                            window.setTimeout(() => {
                                alert(`🎉 Dynamically inserted JavaScript.`);
                            }, 1000);
                        })();
                    "));
            })*/

            /*
            .RegisterWebMessageReceivedHandler((object? sender, string message) =>
            {
                ArgumentNullException.ThrowIfNull(sender);

                var window = (PhotinoWindow)sender;

                // The message argument is coming in from sendMessage.
                // "window.external.sendMessage(message: string)"
                string response = $"Received message: \"{message}\"";

                // Send a message back the to JavaScript event handler.
                // "window.external.receiveMessage(callback: Function)"
                window.SendWebMessage(response);
            })
            */
            .Load($"{baseUrl}/index.html"); // Can be used with relative path strings or "new URI()" instance to load a website.

        window.WaitForClose(); // Starts the application event loop
    }
}

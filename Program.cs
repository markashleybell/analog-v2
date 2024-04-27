using System.Text;
using PhotinoNET;
using PhotinoNET.Server;

namespace Photino.HelloPhotino.Vue;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        PhotinoServer
            .CreateStaticFileServer(args, out string baseUrl)
            .RunAsync();

        var window = new PhotinoWindow()
            .SetTitle("AnaLog 2.0")
            .SetUseOsDefaultSize(false)
            .SetUseOsDefaultLocation(false)
            .SetSize(new System.Drawing.Size(1920, 1440))
            .Center()
            .SetDevToolsEnabled(false)
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

using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.FileProviders;
using PhotinoNET;

namespace Photino.HelloPhotino.Vue;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        var (server, baseUrl) = Infrastructure.CreateLocalServer(args);

        server.RunAsync();

        var window = new PhotinoWindow()
            .SetTitle("AnaLog 2.0")
            .SetUseOsDefaultSize(false)
            .SetUseOsDefaultLocation(false)
            .SetSize(new System.Drawing.Size(1920, 1440))
            .Center()
            // .SetDevToolsEnabled(false)
            .SetResizable(true)
            .Load($"{baseUrl}/index.html");

        window.WaitForClose(); // Starts the application event loop
    }
}

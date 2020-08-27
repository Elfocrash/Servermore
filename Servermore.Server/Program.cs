using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Servermore.Server
{
    public class Program
    {
        //I hate this as much as you do. I'm just messing around
        public static IHost FunctionRunnerHost;
        public static string[] Args;

        public static async Task Main(string[] args)
        {
            Args = args;
            FunctionRunnerHost = CreateServerHostBuilder(args).Build();
            await FunctionRunnerHost.StartAsync();

            MonitorDirectory(FunctionRunnerHost.Services.GetRequiredService<IConfiguration>().GetValue<string>("FunctionLoader:FunctionDirectory"));

            await CreateOrchestratorHostBuilder(args)
                .Build().StartAsync();

            Process.GetCurrentProcess().WaitForExit();
        }

        private static void MonitorDirectory(string path)
        {
            var fileSystemWatcher = new FileSystemWatcher
            {
                Path = path,
                IncludeSubdirectories = true
            };
            fileSystemWatcher.Created += async (sender, args) => { await RestartServer(); };
            fileSystemWatcher.Deleted += async (sender, args) => { await RestartServer(); };
            fileSystemWatcher.EnableRaisingEvents = false;

            static async Task RestartServer()
            {
                await FunctionRunnerHost.StopAsync();
                await FunctionRunnerHost.WaitForShutdownAsync();
                // while (!ArePortsAvailable(5000, 5001))
                // {
                //     await Task.Delay(500);
                // }
                FunctionRunnerHost = CreateServerHostBuilder(Args).Build();
                await FunctionRunnerHost.StartAsync();
            }
        }

        public static IHostBuilder CreateServerHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static IHostBuilder CreateOrchestratorHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<OrchestratorStartup>()
                        .UseUrls("https://localhost:5300");
                });


        public static bool ArePortsAvailable(params int[] ports)
        {
            var portsToCheck = ports.ToList();

            IPEndPoint[] endPoints;
            var portArray = new List<int>();

            var properties = IPGlobalProperties.GetIPGlobalProperties();

            if (properties.GetActiveTcpConnections().Any(x => portsToCheck.Contains(x.LocalEndPoint.Port)))
            {
                return false;
            }

            if (properties.GetActiveTcpListeners().Any(x => portsToCheck.Contains(x.Port)))
            {
                return false;
            }

            if (properties.GetActiveUdpListeners().Any(x => portsToCheck.Contains(x.Port)))
            {
                return false;
            }

            return true;
        }
    }
}

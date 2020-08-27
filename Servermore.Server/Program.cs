using System.Diagnostics;
using System.IO;
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
                Path = path
            };
            fileSystemWatcher.Created += async (sender, args) => { await RestartServer(); };
            fileSystemWatcher.Deleted += async (sender, args) => { await RestartServer(); };
            fileSystemWatcher.EnableRaisingEvents = true;

            static async Task RestartServer()
            {
                await FunctionRunnerHost.StopAsync();
                await FunctionRunnerHost.WaitForShutdownAsync();
                FunctionRunnerHost.Dispose();
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
    }
}

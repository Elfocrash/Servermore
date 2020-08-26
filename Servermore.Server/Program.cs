using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Servermore.Server
{
    public class Program
    {
        private static IHost Server;
        private static string[] Args;

        public static async Task Main(string[] args)
        {
            Args = args;
            //TODO have this check a directory for changes?
            Server = CreateHostBuilder(args).Build();
            await Server.StartAsync();

            MonitorDirectory("F:\\lab\\Servermore\\Servermore.Server\\bin\\loadcation");

            Process.GetCurrentProcess().WaitForExit();
        }

        private static void MonitorDirectory(string path)
        {
            var fileSystemWatcher = new FileSystemWatcher
            {
                Path = path
            };
            fileSystemWatcher.Created += async (sender, args) => { await RestartServer(); };
            fileSystemWatcher.Changed += async (sender, args) => { await RestartServer(); };
            fileSystemWatcher.Deleted += async (sender, args) => { await RestartServer(); };
            fileSystemWatcher.EnableRaisingEvents = true;

            static async Task RestartServer()
            {
                await Server.StopAsync();
                Server = CreateHostBuilder(Args).Build();
                await Server.StartAsync();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}

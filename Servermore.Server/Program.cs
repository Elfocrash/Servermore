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
        private static IHost _server;
        private static string[] _args;

        public static async Task Main(string[] args)
        {
            _args = args;
            //TODO have this check a directory for changes?
            _server = CreateHostBuilder(args).Build();
            await _server.StartAsync();

            MonitorDirectory(_server.Services.GetRequiredService<IConfiguration>().GetValue<string>("FunctionLoader:FunctionDirectory"));

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
                await _server.StopAsync();
                _server = CreateHostBuilder(_args).Build();
                await _server.StartAsync();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

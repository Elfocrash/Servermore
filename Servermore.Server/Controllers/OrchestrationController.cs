using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Servermore.Contracts;
using Servermore.Server.Loader;
using Servermore.Server.Models;

namespace Servermore.Server.Controllers
{
    public class OrchestrationController : ControllerBase
    {
        // private readonly IHost _functionRunnerHost;
        //
        // public OrchestrationController(IHost functionRunnerHost)
        // {
        //     _functionRunnerHost = functionRunnerHost;
        // }

        [HttpGet("orchestrator/restart")]
        public async Task<IActionResult> RestartFunctionRunner()
        {
            //TODO inject this
            await Program.FunctionRunnerHost.StopAsync();
            await Program.FunctionRunnerHost.WaitForShutdownAsync();
            // while (!Program.ArePortsAvailable(5000, 5001))
            // {
            //     await Task.Delay(500);
            // }
            Program.FunctionRunnerHost = Program.CreateServerHostBuilder(Program.Args).Build();
            await Program.FunctionRunnerHost.StartAsync();
            return Ok();
        }

        [HttpPost("orchestrator/upload")]
        public async Task<IActionResult> UploadFunction([FromQuery] string functionName)
        {
            var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();

            await using var ms = new MemoryStream();
            await HttpContext.Request.Body.CopyToAsync(ms);
            await System.IO.File.WriteAllBytesAsync(
                Path.Combine(configuration.GetValue<string>("FunctionLoader:FunctionDirectory"), functionName, $"{functionName}.dll"),
                ms.ToArray());
            //TODO inject this
            await Program.FunctionRunnerHost.StopAsync();
            await Program.FunctionRunnerHost.WaitForShutdownAsync();

            // while (!Program.ArePortsAvailable(5000, 5001))
            // {
            //     await Task.Delay(500);
            // }

            // await Task.Delay(5000);
            Program.FunctionRunnerHost = Program.CreateServerHostBuilder(Program.Args).Build();
            await Program.FunctionRunnerHost.StartAsync();
            return Ok();
        }

        [HttpGet("orchestrator/unload")]
        public async Task<IActionResult> UnloadFunction([FromQuery] string functionName)
        {
            var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var path = Path.Combine(configuration.GetValue<string>("FunctionLoader:FunctionDirectory"),
                functionName, $"{functionName}.dll");

            System.IO.File.Delete(path);

            //TODO inject this
            await Program.FunctionRunnerHost.StopAsync();
            await Program.FunctionRunnerHost.WaitForShutdownAsync();
            // while (!Program.ArePortsAvailable(5000, 5001))
            // {
            //     await Task.Delay(500);
            // }
            Program.FunctionRunnerHost = Program.CreateServerHostBuilder(Program.Args).Build();
            await Program.FunctionRunnerHost.StartAsync();
            return Ok();
        }

        [HttpGet("orchestrator/functions")]
        public IActionResult GetLoadedFunctions()
        {
            //TODO inject this
            var loadedFunctions = FunctionLoaderExtensions.LoadedFunctionMethods.Select(x => new FunctionInfo
            {
                MethodName = x.Name,
                FunctionName = x.GetCustomAttribute<FunctionAttribute>()!.Name,
                TypeName = x.GetCustomAttribute<FunctionAttribute>()!.GetType().Name,
                AssemblyLocation = x.Module.Assembly.FullName
            }).ToList();
            return Ok(loadedFunctions);
        }
    }
}

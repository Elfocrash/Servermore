using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Servermore.Server.Loader;

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
            await Program.FunctionRunnerHost.StopAsync();
            await Program.FunctionRunnerHost.WaitForShutdownAsync();
            Program.FunctionRunnerHost.Dispose();
            // await Task.Delay(5000);
            Program.FunctionRunnerHost = Program.CreateServerHostBuilder(Program.Args).Build();
            await Program.FunctionRunnerHost.StartAsync();
            return Ok();
        }
    }
}

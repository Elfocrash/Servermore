using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Servermore.Contracts;
using Servermore.Sdk;

namespace Servermore.ApiSample
{
    public class ExampleFunctions
    {
        private readonly IFunctionLogger _logger;

        public ExampleFunctions(IFunctionLogger logger)
        {
            _logger = logger;
        }

        [EndpointFunction("SimpleGetEndpoint", "api/test")]
        public async Task<IActionResult> GetEndpointTest()
        {
            _logger.Log("Endpoint called");
            return new OkObjectResult(new { Text = "Did this work?"});
        }

        [EndpointFunction("SimpleGetEndpoint2", "api/test2")]
        public async Task<IActionResult> GetEndpointTest2(HttpContext httpContext)
        {
            _logger.Log($"Endpoint called with query string {httpContext.Request.QueryString}");
            return new OkObjectResult(new { Text = $"Did this work? {httpContext.Request.QueryString}"});
        }
    }
}

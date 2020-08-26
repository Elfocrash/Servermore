using System.Threading.Tasks;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Servermore.Contracts;
using Servermore.Sdk;

namespace Servermore.ApiSample
{
    public class ExampleFunctions
    {
        private readonly IFunctionLogger _logger;
        private readonly IMetricsCollector _metrics;

        public ExampleFunctions(IFunctionLogger logger, IMetricsCollector metrics)
        {
            _logger = logger;
            _metrics = metrics;
        }

        [EndpointFunction("SimpleGetEndpoint", "api/test")]
        public async Task<IActionResult> GetEndpointTest()
        {
            _logger.Log("Endpoint called");
            return new OkObjectResult(new { Text = "Did this work?"});
        }

        [EndpointFunction("SimpleGetEndpoint", "api/oneof")]
        public async Task<IActionResult> TestDepsEndpoint()
        {
            var opt = new Some<string>("work");
            return new OkObjectResult(new { Text = $"Did this {opt.Value}?"});
        }

        [EndpointFunction("TestingServiceLifetime", "api/metric")]
        public async Task<IActionResult> GetMetric()
        {
            _metrics.Increment("Test");
            return new OkObjectResult(new { MetricVal = $"Test metric: {_metrics.GetValue("Test")}"});
        }

        [EndpointFunction("SimpleGetEndpoint2", "api/test2")]
        public async Task<IActionResult> GetEndpointTest2(HttpContext httpContext)
        {
            _logger.Log($"Endpoint called with query string {httpContext.Request.QueryString}");
            return new OkObjectResult(new { Text = $"Did this work? {httpContext.Request.QueryString}"});
        }

        [FunctionServiceConfiguration]
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMetricsCollector, MetricsCollector>();
        }
    }
}

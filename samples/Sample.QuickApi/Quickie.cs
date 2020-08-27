using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Servermore.Contracts;

namespace Sample.QuickApi
{
    public class Quickie
    {
        [EndpointFunction("Testpoint", "tryme")]
        public IActionResult SimpleSync(HttpContext httpContext)
        {
            return new OkObjectResult(new
            {
                Name = $"Your query string was: {httpContext.Request.QueryString}"
            });
        }
    }
}

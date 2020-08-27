using Microsoft.AspNetCore.Mvc;
using Servermore.Contracts;

namespace Sample.QuickApi
{
    public class Quickie
    {
        [EndpointFunction("Testpoint", "tryme")]
        public IActionResult SimpleSync()
        {
            return new OkObjectResult(new
            {
                Name = "Nick"
            });
        }
    }
}

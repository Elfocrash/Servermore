using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Servermore.Contracts;

namespace Servermore.Server.Loader
{
    public class EndpointFunctionLoader
    {
        public static void Load(IApplicationBuilder applicationBuilder, List<MethodInfo> functionAttributeMethods,
            Type functionClassType)
        {
            var endpointFunctionMethods =
                functionAttributeMethods.Where(x => x.GetCustomAttribute<EndpointFunctionAttribute>() != null).ToList();

            foreach (var endpointFunctionMethod in endpointFunctionMethods)
            {
                var endpointInfo = endpointFunctionMethod.GetCustomAttribute<EndpointFunctionAttribute>();
                var shouldAwaitMethod = endpointFunctionMethod.ReturnType.BaseType == typeof(Task);

                //check for duplicate routes here?
                var route = endpointInfo!.Route.StartsWith('/') ? endpointInfo!.Route : $"/{endpointInfo!.Route}";
                applicationBuilder.Map(route, builder =>
                {
                    builder.Run(async context =>
                    {
                        var activatedFunctionClass =  ActivatorUtilities.CreateInstance(applicationBuilder.ApplicationServices, functionClassType);
                        var response = endpointFunctionMethod.GetParameters().Length switch
                        {
                            0 => await InvokeFunctionAppropriately(activatedFunctionClass, shouldAwaitMethod, endpointFunctionMethod, null),
                            //TODO expand on this with more types
                            1 when endpointFunctionMethod.GetParameters()[0].ParameterType == typeof(HttpContext) =>
                                await InvokeFunctionAppropriately(activatedFunctionClass, shouldAwaitMethod, endpointFunctionMethod, new[] {context}),
                            _ => null
                        };

                        switch (response)
                        {
                            case IActionResult actionResult:
                                await ExecuteResultFromActionResult(actionResult, context);
                                return;
                            case null:
                                await WriteNullFallbackResponse(context);
                                return;
                        }

                        await WriteDefaultResponse(context, response);
                    });
                });
            }

            async Task<object> InvokeFunctionAppropriately(object functionClassInstance, bool shouldAwaitMethod, MethodInfo endpointFunctionMethod, object?[] parameters)
            {
                return shouldAwaitMethod
                    ? await ((dynamic) endpointFunctionMethod.Invoke(functionClassInstance, parameters))!
                    : endpointFunctionMethod.Invoke(functionClassInstance, parameters);
            }
        }

        private static async Task ExecuteResultFromActionResult(IActionResult actionResult, HttpContext context)
        {
            await actionResult.ExecuteResultAsync(new ActionContext
            {
                HttpContext = context
            });
        }

        private static async Task WriteDefaultResponse(HttpContext context, object response)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json; charset=utf-8";
            //TODO potentially check if it's primitive
            await context.Response.WriteAsync(JsonSerializer.Serialize(response), Encoding.UTF8);
        }

        private static async Task WriteNullFallbackResponse(HttpContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(string.Empty, Encoding.UTF8);
        }
    }
}

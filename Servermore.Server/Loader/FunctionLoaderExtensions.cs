using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Servermore.Contracts;

namespace Servermore.Server.Loader
{
    public static class FunctionLoaderExtensions
    {
        public static IApplicationBuilder UseServermore(this IApplicationBuilder applicationBuilder, IConfiguration configuration)
        {
            var ass = LoadPlugin(configuration.GetValue<string>("FunctionLoader:FunctionDirectory"));

            var exportedTypes = ass.SelectMany(x => x.ExportedTypes).ToList();

            foreach (var exportedType in exportedTypes)
            {
                var functionAttributeMethods = exportedType.GetMethods()
                    .Where(x => x.GetCustomAttribute<FunctionAttribute>() != null)
                    .ToList();

                if (functionAttributeMethods.Count == 0)
                {
                    continue;
                }

                var activatedClass = ActivatorUtilities.CreateInstance(applicationBuilder.ApplicationServices, exportedType);

                EndpointFunctionLoader.Load(applicationBuilder, functionAttributeMethods, activatedClass);
            }
            return applicationBuilder;
        }

        private static List<Assembly> LoadPlugin(string rootPath)
        {
            var list = new List<Assembly>();

            var dlls = Directory.EnumerateFiles(rootPath, "*.dll", SearchOption.AllDirectories);

            foreach (var dll in dlls)
            {
                Console.WriteLine($"Loading commands from: {dll}");
                var loadContext = new FunctionLoadContext(dll);
                list.Add(loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(dll))));
            }

            return list;
        }
    }
}

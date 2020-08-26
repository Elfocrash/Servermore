using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Servermore.Contracts;

namespace Servermore.Server.Loader
{
    public static class FunctionLoaderExtensions
    {
        public static List<Assembly> LoadedAssemblies = new List<Assembly>();

        public static IApplicationBuilder UseServermore(this IApplicationBuilder applicationBuilder, IConfiguration configuration)
        {
            var exportedTypes = LoadedAssemblies.SelectMany(x => x.ExportedTypes).ToList();

            foreach (var exportedType in exportedTypes)
            {
                var functionAttributeMethods = exportedType.GetMethods()
                    .Where(x => x.GetCustomAttribute<FunctionAttribute>() != null)
                    .ToList();

                if (functionAttributeMethods.Count == 0)
                {
                    continue;
                }

                EndpointFunctionLoader.Load(applicationBuilder, functionAttributeMethods, exportedType);
            }
            return applicationBuilder;
        }

        public static IServiceCollection AddServermore(this IServiceCollection services, string functionLoadPath)
        {
            var exportedTypes = LoadedAssemblies.SelectMany(x => x.ExportedTypes).ToList();

            foreach (var exportedType in exportedTypes)
            {
                ConfigureFunctionServices(services, exportedType);
            }

            return services;
        }

        private static void ConfigureFunctionServices(IServiceCollection services, Type exportedType)
        {
            var serviceConfigurationMethods = exportedType.GetMethods()
                .Where(x => x.GetCustomAttribute<FunctionServiceConfigurationAttribute>() != null && x.IsStatic)
                .ToList();

            if (serviceConfigurationMethods.Count == 0)
            {
                return;
            }

            serviceConfigurationMethods.ForEach(info => { info.Invoke(null, new [] {services}); });
        }
    }
}

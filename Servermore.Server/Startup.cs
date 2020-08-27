using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Servermore.Sdk;
using Servermore.Server.Loader;

namespace Servermore.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            FunctionLoaderExtensions.LoadedAssemblies = LoadFunctionAssemblies(configuration.GetValue<string>("FunctionLoader:FunctionDirectory"));
        }

        private static List<Assembly> LoadFunctionAssemblies(string rootPath)
        {
            var list = new List<Assembly>();

            var dlls = Directory.EnumerateFiles(rootPath, "*.dll", SearchOption.AllDirectories);

            foreach (var dll in dlls)
            {
                Console.WriteLine($"Loading functions from: {dll}");
                var loader = new FunctionLoadContext(dll);
                list.Add(loader.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(dll))));
            }

            return list;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFunctionLogger, FunctionLogger>();
            services.AddServermore(Configuration.GetValue<string>("FunctionLoader:FunctionDirectory"));
            services.AddMvcCore();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseServermore(Configuration);
        }
    }
}

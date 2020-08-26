using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Servermore.Contracts;
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
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseServermore(Configuration);
        }
    }
}

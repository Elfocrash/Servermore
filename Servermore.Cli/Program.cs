using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Servermore.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //TODO commands

            //1. dotnet servermore pack
            //    - Build the project
            //    - Pick the artifacts and the dependency dlls
            //    - Package them up in a zip

            //2. dotnet servermore deploy --local
            //    - Will pack and paste the zip at the specified directory

            //3. dotnet servermore deploy --remote
            //    - Will pack and upload the zip at the specified Servermore server

            //Testing around

            var command = args[0];
            var url = args[1];

            var httpClient = new HttpClient();
            switch (command)
            {
                case "deploy":
                {
                    var functionName = args[2];
                    Console.WriteLine($"Deploying Servermore function {functionName}");
                    await using var fileStream = File.OpenRead($"F:\\lab\\Servermore\\samples\\{functionName}\\bin\\Debug\\netcoreapp3.1\\{functionName}.dll");
                    await httpClient.PostAsync($"{url}/orchestrator/upload?functionName={WebUtility.UrlEncode(functionName)}", new StreamContent(fileStream));
                    Console.WriteLine($"{functionName} function deployed successfully");
                    break;
                }
                case "unload":
                    var functionNameToUnload = args[2];
                    Console.WriteLine($"Unloading Servermore function {functionNameToUnload}");
                    await httpClient.GetAsync($"{url}/orchestrator/unload?functionName={WebUtility.UrlEncode(functionNameToUnload)}");
                    Console.WriteLine($"{functionNameToUnload} function unloaded successfully");
                    break;
                case "ls":
                    var response = await httpClient.GetStringAsync($"{url}/orchestrator/functions");
                    var desir = JsonSerializer.Deserialize<object>(response);

                    Console.WriteLine(JsonSerializer.Serialize(desir, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));
                    break;
                default:
                    return;
            }
        }
    }
}

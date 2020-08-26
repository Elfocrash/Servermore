using System;

namespace Servermore.Cli
{
    class Program
    {
        static void Main(string[] args)
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
        }
    }
}

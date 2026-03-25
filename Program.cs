using Microsoft.Extensions.DependencyInjection; // Fixes: AddSingleton error
using Microsoft.Extensions.Hosting;             // Fixes: CreateApplicationBuilder
using ModelContextProtocol.Server;              // Fixes: AddMcpServer error
using System.ComponentModel;

namespace MCPSolveIT
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Register the data store as a Singleton so it only loads once
            //builder.Services.AddSingleton<TelemetryDataStore>();

            builder.Services.AddMcpServer()
                   .WithStdioServerTransport()
                   .WithToolsFromAssembly();

            await builder.Build().RunAsync();
        }
    }
}

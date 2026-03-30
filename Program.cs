using Microsoft.Extensions.DependencyInjection; 
using Microsoft.Extensions.Hosting;             
using ModelContextProtocol.Server;              
using System.ComponentModel;

namespace MCPSolveIT
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register the data store as a Singleton so it only loads once
            //builder.Services.AddSingleton<TelemetryDataStore>();

            builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly();

            var app = builder.Build();

            // --- 🛡️ UPDATED AUTHENTICATION MIDDLEWARE ---
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/mcp"))
                {
                    var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                    bool isAuthorized = false;

                    // 1. Check if the header exists and starts with "Bearer "
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        // Extract just the key string
                        var extractedKey = authHeader.Substring("Bearer ".Length).Trim();

                        // 2. Load the list of valid keys from appsettings.json
                        var validKeys = app.Configuration.GetSection("McpApiKeys").Get<List<ApiKeyConfig>>()
                                        ?? new List<ApiKeyConfig>();

                        // 3. Check if the extracted key matches ANY key in our list
                        var matchedClient = validKeys.FirstOrDefault(k => k.Key == extractedKey);

                        if (matchedClient != null)
                        {
                            isAuthorized = true;
                            // Optional: Log which specific client just connected!
                            app.Logger.LogInformation("MCP Client Connected: {ClientName}", matchedClient.Name);
                        }
                    }

                    // 4. Reject the request if no match was found
                    if (!isAuthorized)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized: Invalid or missing API Key.");
                        return; // Stop the pipeline
                    }
                }

                // If authorized, proceed to the MCP endpoints
                await next(context);
            });
            // ---------------------------------------------

            app.MapMcp("/mcp");
            app.MapGet("/", () => Results.Ok(new
            { 
                service = "MCPSolveIT",
                transport = "http",
                mcpEndpoint = "/mcp",
            }
            ));

            await app.RunAsync();
        }
    }
}

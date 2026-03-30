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

            builder.Services.AddHttpContextAccessor(); // Needed to access HttpContext in tools

            builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly();

            var app = builder.Build();

            // --- 🛡️ UPDATED AUTHENTICATION MIDDLEWARE ---
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/mcp"))
                {
                    // 1. Read the flag (Defaults to 'true' if the setting is missing from JSON)
                    bool authRequired = app.Configuration.GetValue<bool>("AuthenticationRequired", true);

                    if (authRequired)
                    {
                        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                        bool isAuthorized = false;

                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                        {
                            var extractedKey = authHeader.Substring("Bearer ".Length).Trim();
                            var validKeys = app.Configuration.GetSection("McpApiKeys").Get<List<ApiKeyConfig>>()
                                            ?? new List<ApiKeyConfig>();

                            var matchedClient = validKeys.FirstOrDefault(k => k.Key == extractedKey);

                            if (matchedClient != null)
                            {
                                isAuthorized = true;
                                context.Items["McpClientName"] = matchedClient.Name;
                            }
                        }

                        if (!isAuthorized)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync("Unauthorized: Invalid or missing API Key.");
                            return; // Stop the pipeline
                        }
                    }
                    else
                    {
                        // 2. If Auth is turned off, flag them as Anonymous so your tools still work!
                        context.Items["McpClientName"] = "Anonymous (Auth Disabled)";
                        app.Logger.LogWarning("MCP Request permitted without authentication.");
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

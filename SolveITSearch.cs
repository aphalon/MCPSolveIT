using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MCPSolveIT
{
    [McpServerToolType]
    internal class SolveITSearch

    {

        // Inject both stores into the constructor
        public SolveITSearch()
        {
        }

        // --- TOOL 1: TELEMETRY ---
        [McpServerTool(Name = "search_telemetry_data", Title = "Searches serialized JSON data for telemetry based on specific criteria.")]
        public string SearchTelemetryData(
            [Description("Specific key or ID to match.")] string? key = null,
            [Description("Severity level.")] string? severity = null)
        {
            //var query = _telemetryStore.Records.AsQueryable();

            // ... apply telemetry filters ...

            return "{}";// JsonSerializer.Serialize(query.Take(50).ToList());
        }

        // --- TOOL 2: ASSETS ---
        [McpServerTool(Name = "search_asset_data",Title = "Searches serialized JSON data for assets based on specific criteria.")]
        public string SearchAssetData(
            [Description("The asset ID to find.")] string? assetId = null,
            [Description("The current status of the asset (e.g., 'Active', 'Retired').")] string? status = null)
        {
            //var query = _assetStore.Records.AsQueryable();

            //// ... apply asset filters ...
            //if (!string.IsNullOrEmpty(assetId)) query = query.Where(a => a.AssetId == assetId);
            //if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);

            return "{}";//return JsonSerializer.Serialize(query.Take(50).ToList());
        }
    }
}
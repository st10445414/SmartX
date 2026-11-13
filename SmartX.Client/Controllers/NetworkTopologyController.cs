
using Microsoft.AspNetCore.Mvc;
using SmartX.Client.Models;
using SmartX.Client.Services;
using System.Text.Json;

namespace SmartX.Client.Controllers
{
    public class NetworkTopologyController : Controller
    {
        private readonly IMeshTopologyApiService _meshApi;

        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        public NetworkTopologyController(
            IMeshTopologyApiService meshApi)
        {
            _meshApi = meshApi;
        }

        // GET: /NetworkTopology
        public async Task<IActionResult> Index(
            string? sourceMac,
            string? destinationMac)
        {
            var model = new NetworkTopologyViewModel
            {
                SourceMac = sourceMac ?? string.Empty,
                DestinationMac = destinationMac ?? string.Empty
            };

            try
            { 
                // Retrieve Prim's MST.
                var mst = await _meshApi
                    .GetMinimumSpanningTreeAsync();

                model.MinimumSpanningTree =
                    ReadList<MeshEdgeViewModel>(
                        mst, "connections");

                // Retrieve the complete network.
                var topology = await _meshApi.GetTopologyAsync();

                model.Nodes = ReadList<MeshNodeViewModel>(
                    topology, "nodes");

                model.Edges = ReadList<MeshEdgeViewModel>(
                    topology, "edges");

                // Retrieve unreachable devices.
                var unreachable = await _meshApi
                    .GetUnreachableSensorsAsync();

                model.UnreachableSensors =
                    ReadList<MeshNodeViewModel>(
                        unreachable, "unreachableSensors");

                // Only trace a route when both MACs
                // have been supplied.
                if (!string.IsNullOrWhiteSpace(sourceMac) &&
                    !string.IsNullOrWhiteSpace(destinationMac))
                {
                    var routeResult =
                        await _meshApi.TraceRouteAsync(
                            sourceMac, destinationMac);

                    if (routeResult.TryGetProperty(
                        "routeFound", out var found))
                    {
                        model.RouteFound =
                            found.ValueKind == JsonValueKind.True;
                    }

                    model.Route = ReadList<MeshNodeViewModel>(
                        routeResult, "route");
                }

                // Convert numeric API enum values into
                // readable sensor status labels.
                foreach (var node in model.Nodes)
                {
                    node.Status = NormalizeStatus(node.Status);
                }

                foreach (var node in model.UnreachableSensors)
                {
                    node.Status = NormalizeStatus(node.Status);
                }

                foreach (var node in model.Route)
                {
                    node.Status = NormalizeStatus(node.Status);
                }
            }

            catch (HttpRequestException ex)
            {
                model.ErrorMessage =
                    $"Smart-X API request failed: {ex.Message}";
            }

            catch (JsonException)
            {
                model.ErrorMessage =
                    "The network API returned data in an " +
                    "unexpected format.";
            }

            return View(model);
        }

        private static List<T> ReadList<T>(
            JsonElement json, string propertyName)
        {
            if (json.ValueKind != JsonValueKind.Object ||
                !json.TryGetProperty(
                    propertyName, out var property) ||
                property.ValueKind != JsonValueKind.Array)
            {
                return new List<T>();
            }

            return JsonSerializer.Deserialize<List<T>>(
                property.GetRawText(), JsonOptions)
                ?? new List<T>();
        }

        private static string NormalizeStatus(string? status)
        {
            return status switch
            {
                "0" => "Connected",
                "1" => "Warning",
                "2" => "Disconnected",
                null or "" => "Unknown",
                _ => status
            };
        }


        // POST: /NetworkTopology/FailConnection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FailConnection(
            int sourceId,
            int destinationId)
        {
            try
            {
                await _meshApi.SimulateLinkFailureAsync(
                    sourceId, destinationId);

                TempData["SuccessMessage"] =
                    $"Connection {sourceId} → {destinationId} " +
                    "has been disabled.";
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to disable the connection. " +
                    "Check that it exists and is currently active.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /NetworkTopology/RestoreConnection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConnection(
            int sourceId,
            int destinationId)
        {
            try
            {
                await _meshApi.RestoreLinkAsync(
                    sourceId, destinationId);

                TempData["SuccessMessage"] =
                    $"Connection {sourceId} → {destinationId} " +
                    "has been restored.";
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to restore the connection. " +
                    "Check that it exists and is currently inactive.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

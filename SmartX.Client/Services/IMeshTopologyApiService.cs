
using System.Text.Json;

namespace SmartX.Client.Services
{


    public interface IMeshTopologyApiService
    {
        Task<JsonElement> SimulateLinkFailureAsync(
            int sourceId,
            int destinationId);

        Task<JsonElement> RestoreLinkAsync(
            int sourceId,
            int destinationId);

        Task<JsonElement> GetTopologyAsync();

        Task<JsonElement> GetMinimumSpanningTreeAsync();

        Task<JsonElement> GetUnreachableSensorsAsync();

        Task<JsonElement> TraceRouteAsync(
            string sourceMac,
            string destinationMac);
    }
}

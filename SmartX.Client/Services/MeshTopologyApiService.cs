
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartX.Client.Services
{
    public class MeshTopologyApiService
        : IMeshTopologyApiService
    {
        private readonly HttpClient _httpClient;

        public MeshTopologyApiService(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<JsonElement> GetTopologyAsync()
        {
            return await GetJsonAsync(
                "api/MeshTopology");
        }

        public async Task<JsonElement>
            GetMinimumSpanningTreeAsync()
        {
            return await GetJsonAsync(
                "api/MeshTopology/mst");
        }

        public async Task<JsonElement>
            GetUnreachableSensorsAsync()
        {
            return await GetJsonAsync(
                "api/MeshTopology/unreachable");
        }

        public async Task<JsonElement> TraceRouteAsync(
            string sourceMac,
            string destinationMac)
        {
            string source = Uri.EscapeDataString(sourceMac);
            string destination =
                Uri.EscapeDataString(destinationMac);

            return await GetJsonAsync(
                $"api/MeshTopology/route" +
                $"?sourceMac={source}" +
                $"&destinationMac={destination}");
        }

        private async Task<JsonElement> GetJsonAsync(
            string endpoint)
        {
            using var response =
                await _httpClient.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content
                    .ReadFromJsonAsync<JsonElement>();

            return result;
        }


        public async Task<JsonElement> SimulateLinkFailureAsync(
            int sourceId,
            int destinationId)
        {
            return await PostJsonAsync(
                $"api/MeshTopology/fail/{sourceId}/{destinationId}");
        }

        public async Task<JsonElement> RestoreLinkAsync(
            int sourceId,
            int destinationId)
        {
            return await PostJsonAsync(
                $"api/MeshTopology/restore/{sourceId}/{destinationId}");
        }

        private async Task<JsonElement> PostJsonAsync(
            string endpoint)
        {
            using var response = await _httpClient.PostAsync(
                endpoint,
                content: null);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<JsonElement>();
        }

    }
}

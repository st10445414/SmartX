using System.Net.Http.Json;
using System.Text.Json;

namespace SmartX.Client.Services
{
    public class TelemetryHistoryApiService
        : ITelemetryHistoryApiService
    {
        private readonly HttpClient _httpClient;

        public TelemetryHistoryApiService(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<JsonElement>
            GetHistoricalReadingsAsync(
                int sensorId,
                DateTime start,
                DateTime end)
        {
            string startValue =
                Uri.EscapeDataString(
                    start.ToString("O"));

            string endValue =
                Uri.EscapeDataString(
                    end.ToString("O"));

            string endpoint =
                $"api/TelemetryHistory/sensor/{sensorId}" +
                $"?start={startValue}&end={endValue}";

            using var response =
                await _httpClient.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<JsonElement>();
        }
    }
}

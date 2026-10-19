using System.Net.Http.Json;
using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public class InsightsApiService : IInsightsApiService
    {
        private readonly HttpClient _httpClient;

        public InsightsApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SuggestedActionViewModel>>
            GetSuggestionsAsync()
        {
            var response = await _httpClient
                .GetFromJsonAsync<InsightsResponseViewModel>(
                    "api/insights/suggestions");

            return response?.Suggestions ?? new();
        }

        public async Task<bool> RecordActivityAsync(
            int sensorId,
            string activityType,
            string detail)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/insights/activity",
                new
                {
                    SensorId = sensorId,
                    ActivityType = activityType,
                    Detail = detail,
                    Timestamp = DateTime.UtcNow
                });

            return response.IsSuccessStatusCode;
        }
    }
}

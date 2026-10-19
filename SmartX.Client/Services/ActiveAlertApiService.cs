
using System.Net.Http.Json;
using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public class ActiveAlertApiService : IActiveAlertApiService
    {
        private readonly HttpClient _httpClient;

        public ActiveAlertApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ActiveAlertsResponse?> GetAlertsAsync()
        {
            return await _httpClient
                .GetFromJsonAsync<ActiveAlertsResponse>(
                    "api/DataStructures/alerts");
        }
    }
}

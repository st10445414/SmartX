using System.Text.Json;

namespace SmartX.Client.Services
{
    public interface ITelemetryHistoryApiService
    {
        Task<JsonElement> GetHistoricalReadingsAsync(
            int sensorId,
            DateTime start,
            DateTime end);
    }
}

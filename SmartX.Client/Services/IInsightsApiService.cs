using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public interface IInsightsApiService
    {
        Task<List<SuggestedActionViewModel>> GetSuggestionsAsync();

        Task<bool> RecordActivityAsync(
            int sensorId,
            string activityType,
            string detail);
    }
}

using SmartX.Api.Models;

namespace SmartX.Api.Repositories
{
    public interface ITelemetryRepository
    {
        Task<TelemetryRecord> AddAsync(
            TelemetryRecord record);

        Task<List<TelemetryRecord>> GetAllAsync();

        Task<List<TelemetryRecord>> GetBySensorIdAsync(
            int sensorId);
    }
}
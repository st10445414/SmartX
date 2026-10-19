
using SmartX.Api.Models;

namespace SmartX.Api.Repositories
{
    public class TelemetryRepository : ITelemetryRepository
    {
        private readonly List<TelemetryRecord> _records = new();
        private readonly object _sync = new();

        public Task<TelemetryRecord> AddAsync(TelemetryRecord record)
        {
            lock (_sync)
            {
                _records.Add(record);
            }

            return Task.FromResult(record);
        }


        public Task<List<TelemetryRecord>> GetAllAsync()
        {
            lock (_sync)
            {
                return Task.FromResult(_records.ToList());
            }
        }

        public Task<List<TelemetryRecord>> GetBySensorIdAsync(
            int sensorId)
        {
            lock (_sync)
            {
                return Task.FromResult(
                    _records
                        .Where(r => r.SensorId == sensorId)
                        .ToList());
            }
        }
    }
}

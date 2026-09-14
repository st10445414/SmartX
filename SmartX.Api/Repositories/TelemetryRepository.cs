using SmartX.Api.Models;

namespace SmartX.Api.Repositories
{
    public class TelemetryRepository :
        ITelemetryRepository
    {
        private readonly List<TelemetryRecord>
            _telemetryRecords = new();

        public Task<TelemetryRecord> AddAsync(
            TelemetryRecord record)
        {
            _telemetryRecords.Add(record);

            return Task.FromResult(record);
        }

        public Task<List<TelemetryRecord>>
            GetAllAsync()
        {
            return Task.FromResult(
                _telemetryRecords.ToList());
        }

        public Task<List<TelemetryRecord>>
            GetBySensorIdAsync(
                int sensorId)
        {
            var records =
                _telemetryRecords
                    .Where(t =>
                        t.SensorId == sensorId)
                    .OrderByDescending(t =>
                        t.Timestamp)
                    .ToList();

            return Task.FromResult(records);
        }
    }
}

using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Services
{
    public class TelemetryTimelineService
    {
        private readonly ITelemetryRepository _telemetryRepository;

        public TelemetryTimelineService(
            ITelemetryRepository telemetryRepository)
        {
            _telemetryRepository = telemetryRepository;
        }

        public async Task<object> GetTimelineAsync(int sensorId)
        {
            var records = await _telemetryRepository
                .GetBySensorIdAsync(sensorId);

            var timeline =
                new SortedDictionary<DateTime, List<TelemetryRecord>>();

            foreach (var record in records)
            {
                if (!timeline.TryGetValue(
                    record.Timestamp, out var readings))
                {
                    readings = new List<TelemetryRecord>();
                    timeline[record.Timestamp] = readings;
                }

                readings.Add(record);
            }

            var orderedRecords = timeline
                .SelectMany(entry => entry.Value)
                .ToList();

            return new
            {
                SensorId = sensorId,
                TotalRecords = orderedRecords.Count,
                UniqueTimestamps = timeline.Count,
                Records = orderedRecords
            };
        }
    }
}

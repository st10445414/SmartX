
using SmartX.Api.DataStructures;
using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Services
{
    public class TelemetryAvlService
    {
        private readonly ITelemetryRepository _repository;

        public TelemetryAvlService(
            ITelemetryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TelemetryRecord>>
            SearchHistoricalReadingsAsync(
                int sensorId,
                DateTime start,
                DateTime end)
        {
            if (sensorId <= 0)
            {
                throw new ArgumentException(
                    "A valid sensor ID is required.");
            }

            if (start > end)
            {
                throw new ArgumentException(
                    "Start date cannot be after end date.");
            }

            // Retrieve actual stored sensor telemetry.
            var readings = await _repository
                .GetBySensorIdAsync(sensorId);

            // Build a timestamp-indexed AVL tree.
            var tree = new AvlTree<TelemetryRecord>();

            foreach (var reading in readings)
            {
                tree.Insert(
                    reading.Timestamp,
                    reading);
            }

            // Use the AVL tree for the range search.
            return tree.SearchRange(start, end);
        }
    }
}



using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Services
{
    public class PredictiveActionService
    {
        private readonly ITelemetryRepository _telemetryRepository;
        private readonly ISensorRepository _sensorRepository;

        private readonly List<OperatorActivity> _activities = new();
        private readonly object _sync = new();

        public PredictiveActionService(
            ITelemetryRepository telemetryRepository,
            ISensorRepository sensorRepository)
        {
            _telemetryRepository = telemetryRepository;
            _sensorRepository = sensorRepository;
        }

        public void RecordActivity(OperatorActivity activity)
        {
            lock (_sync)
            {
                _activities.Add(new OperatorActivity
                {
                    SensorId = activity.SensorId,
                    ActivityType = activity.ActivityType,
                    Detail = activity.Detail,
                    Timestamp = DateTime.UtcNow
                });

                // Keep only the latest 2,000 activities.
                if (_activities.Count > 2000)
                {
                    _activities.RemoveRange(
                        0, _activities.Count - 2000);
                }
            }
        }

        public async Task<List<SuggestedAction>> GetSuggestionsAsync()
        {
            var sensors =
                await _sensorRepository.GetAllAsync();

            var telemetry =
                await _telemetryRepository.GetAllAsync();

            List<OperatorActivity> activities;

            lock (_sync)
            {
                activities = _activities.ToList();
            }

            var suggestions = new List<SuggestedAction>();

            foreach (var sensor in sensors)
            {
                // Get the latest reading for each metric.
                var latestReadings = telemetry
                    .Where(t => t.SensorId == sensor.Id)
                    .GroupBy(t =>
                        t.MetricName.Trim().ToLowerInvariant())
                    .Select(g => g
                        .OrderByDescending(t => t.Timestamp)
                        .First())
                    .ToList();

                foreach (var reading in latestReadings)
                {
                    string metric =
                        reading.MetricName.Trim().ToLowerInvariant();

                   
                    // LOW MOISTURE PREDICTION
                   
                    if (metric.Contains("moisture") &&
                        reading.Value < 20)
                    {
                        var recentMoistureReadings = telemetry
                            .Where(t =>
                                t.SensorId == sensor.Id &&
                                t.MetricName.Contains(
                                    "moisture",
                                    StringComparison.OrdinalIgnoreCase))
                            .OrderByDescending(t => t.Timestamp)
                            .Take(5)
                            .ToList();

                        double averageMoisture =
                            recentMoistureReadings.Average(
                                t => t.Value);

                        bool sustainedLowMoisture =
                            recentMoistureReadings.Count >= 3 &&
                            recentMoistureReadings.All(
                                t => t.Value < 20);

                        string trendDescription =
                            sustainedLowMoisture
                                ? "Multiple recent readings indicate " +
                                  "sustained low moisture."
                                : "The latest reading indicates " +
                                  "low moisture.";

                        int relevantCommands = activities.Count(a =>
                            a.SensorId == sensor.Id &&
                            a.ActivityType == "Command" &&
                            a.Detail.Contains(
                                "Pump",
                                StringComparison.OrdinalIgnoreCase));

                        int relevantSearches = activities.Count(a =>
                            a.SensorId == sensor.Id &&
                            a.ActivityType == "Search");

                        int confidenceScore = Math.Min(
                            95,
                            60 +
                            relevantCommands * 5 +
                            relevantSearches * 2 +
                            (sustainedLowMoisture ? 10 : 0));

                        suggestions.Add(new SuggestedAction
                        {
                            Id = $"moisture-{sensor.Id}",
                            SensorId = sensor.Id,
                            DeviceName = sensor.DeviceName,
                            Title = "Low moisture detected",

                            Reason =
                                $"Latest moisture reading: " +
                                $"{reading.Value:F1}%. " +
                                $"Average of the last " +
                                $"{recentMoistureReadings.Count} " +
                                $"readings: {averageMoisture:F1}%. " +
                                $"{trendDescription} " +
                                $"Recorded searches: " +
                                $"{relevantSearches}; " +
                                $"pump commands: " +
                                $"{relevantCommands}. " +
                                "Inspect the irrigation system " +
                                "before activating it.",

                            Severity = "Critical",
                            Action = "Pump",
                            Value = "ON",
                            ConfidenceScore = confidenceScore
                        });
                    }

                    
                    // HIGH TEMPERATURE DETECTION
                    
                    if (metric.Contains("temperature") &&
                        reading.Value > 40)
                    {
                        suggestions.Add(new SuggestedAction
                        {
                            Id = $"temperature-{sensor.Id}",
                            SensorId = sensor.Id,
                            DeviceName = sensor.DeviceName,
                            Title = "High temperature detected",

                            Reason =
                                $"Latest temperature: " +
                                $"{reading.Value:F1}. " +
                                "Inspect the device and consider " +
                                "isolating it.",

                            Severity = "Critical",
                            Action = "Relay",
                            Value = "OFF",
                            ConfidenceScore = 90
                        });
                    }

                   
                    // HIGH POWER CONSUMPTION
                   
                    if (metric.Contains("power") &&
                        reading.Value > 5000)
                    {
                        suggestions.Add(new SuggestedAction
                        {
                            Id = $"power-{sensor.Id}",
                            SensorId = sensor.Id,
                            DeviceName = sensor.DeviceName,
                            Title = "High power consumption",

                            Reason =
                                $"Latest power reading: " +
                                $"{reading.Value:F1}. " +
                                "Check the connected equipment.",

                            Severity = "Warning",
                            Action = "Relay",
                            Value = "OFF",
                            ConfidenceScore = 85
                        });
                    }
                }

               
                // DISCONNECTED DEVICE
               
                if (sensor.Status == SensorStatus.Disconnected)
                {
                    suggestions.Add(new SuggestedAction
                    {
                        Id = $"disconnected-{sensor.Id}",
                        SensorId = sensor.Id,
                        DeviceName = sensor.DeviceName,
                        Title = "Device disconnected",

                        Reason =
                            "The device has not reported recently. " +
                            "Check connectivity before attempting " +
                            "any remote control.",

                        Severity = "Warning",
                        Action = "Relay",
                        Value = "OFF",
                        ConfidenceScore = 90
                    });
                }
            }

            // Critical recommendations appear first.
            return suggestions
                .OrderByDescending(s =>
                    s.Severity == "Critical")
                .ThenByDescending(s =>
                    s.ConfidenceScore)
                .ToList();
        }
    }
}

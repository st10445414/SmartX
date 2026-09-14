namespace SmartX.Api.Models
{
    public class TelemetryRecord
    {
        public int SensorId { get; set; }

        public string MetricName { get; set; } = string.Empty;

        public double Value { get; set; }

        public string Unit { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsAnomaly { get; set; }

        public string AnomalyReason { get; set; } = string.Empty;
    }
}
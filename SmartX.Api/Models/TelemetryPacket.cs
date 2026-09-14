namespace SmartX.Api.Models
{
    public class TelemetryPacket<T>
    {
        public int SensorId { get; set; }

        public string MetricName { get; set; } = string.Empty;

        public T? Value { get; set; }

        public string Unit { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsAnomaly { get; set; }

        public string? AnomalyReason { get; set; }
    }
}
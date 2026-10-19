
namespace SmartX.Client.Models
{
    public class DeviceDetailsViewModel
    {
        public int SensorId { get; set; }

        public string DeviceName { get; set; } = string.Empty;

        public string MacAddress { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Zone { get; set; } = string.Empty;

        public string DeploymentLocation { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public List<DeviceTelemetryViewModel> TelemetryHistory
        { get; set; } = new();

        public string? ErrorMessage { get; set; }
    }

    public class DeviceTelemetryViewModel
    {
        public string MetricName { get; set; } = string.Empty;

        public double Value { get; set; }

        public string Unit { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public bool IsAnomaly { get; set; }

        public string? AnomalyReason { get; set; }
    }
}

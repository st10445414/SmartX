using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.Models
{
    public class TelemetryIngestionRequest
    {
        [Range(1, int.MaxValue)]
        public int SensorId { get; set; }

        [Required]
        public string MetricName { get; set; } = string.Empty;

        public double Value { get; set; }

        [Required]
        public string Unit { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

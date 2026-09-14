using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.Models
{
    public class TelemetryRequest
    {
        [Required]
        public int SensorId { get; set; }

        [Required]
        public string MetricName { get; set; } = string.Empty;

        [Required]
        public double Value { get; set; }

        [Required]
        public string Unit { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
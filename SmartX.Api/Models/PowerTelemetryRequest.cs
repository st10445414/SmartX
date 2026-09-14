using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.Models
{
    public class PowerTelemetryRequest
    {
        [Required]
        public int SensorId { get; set; }

        [Required]
        public string MetricName { get; set; } = "Power";

        public int Value { get; set; }

        [Required]
        public string Unit { get; set; } = "W";

        public DateTime Timestamp { get; set; }
            = DateTime.UtcNow;
    }
}
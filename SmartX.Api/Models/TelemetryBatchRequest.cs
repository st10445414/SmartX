using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.Models
{
    public class TelemetryBatchRequest
    {
        [Required]
        public int SensorId { get; set; }

        [Required]
        public string MetricName { get; set; } = string.Empty;

        [Required]
        public string Unit { get; set; } = string.Empty;

        [Range(1, 10000)]
        public int BatchSize { get; set; }

        public double MinimumValue { get; set; }

        public double MaximumValue { get; set; }
    }
}
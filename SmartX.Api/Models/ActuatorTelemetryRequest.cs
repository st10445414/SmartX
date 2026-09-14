using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.Models
{
    public class ActuatorTelemetryRequest
    {
        [Required]
        public int SensorId { get; set; }

        [Required]
        public string MetricName { get; set; }
            = "ValveState";

        public bool Value { get; set; }

        public DateTime Timestamp { get; set; }
            = DateTime.UtcNow;
    }
}
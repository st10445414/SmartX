
namespace SmartX.Api.Models
{
    public class DeviceCommand
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int SensorId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public bool IsCritical { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public string Status { get; set; } = "Queued";
    }
}

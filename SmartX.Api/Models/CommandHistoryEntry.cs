
namespace SmartX.Api.Models
{
    public class CommandHistoryEntry
    {
        public Guid CommandId { get; set; }

        public int SensorId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string PreviousValue { get; set; } = string.Empty;

        public string NewValue { get; set; } = string.Empty;

        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UndoneAt { get; set; }
    }
}

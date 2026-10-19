
namespace SmartX.Client.Models
{
    public class CommandStatusViewModel
    {
        public int NormalQueueCount { get; set; }

        public int CriticalQueueCount { get; set; }

        public int UndoCount { get; set; }

        public int ProcessedCount { get; set; }

        public Dictionary<string, string> DeviceStates { get; set; }
            = new();

        public List<ProcessedCommandViewModel> RecentCommands { get; set; }
            = new();
    }

    public class ProcessedCommandViewModel
    {
        public Guid Id { get; set; }

        public int SensorId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public bool IsCritical { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? ProcessedAt { get; set; }
    }
}

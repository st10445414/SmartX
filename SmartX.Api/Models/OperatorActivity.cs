namespace SmartX.Api.Models
{
    public class OperatorActivity
    {
        public int SensorId { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

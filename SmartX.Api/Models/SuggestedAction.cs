namespace SmartX.Api.Models
{
    public class SuggestedAction
    {
        public string Id { get; set; } = string.Empty;
        public int SensorId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Severity { get; set; } = "Info";
        public string Action { get; set; } = "Pump";
        public string Value { get; set; } = "ON";
        public int ConfidenceScore { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
namespace SmartX.Client.Models
{
    public class SuggestedActionViewModel
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
        public DateTime GeneratedAt { get; set; }
    }

    public class InsightsResponseViewModel
    {
        public int Count { get; set; }

        public List<SuggestedActionViewModel> Suggestions
        { get; set; } = new();
    }
}

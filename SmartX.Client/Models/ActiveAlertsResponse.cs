namespace SmartX.Client.Models
{
    public class ActiveAlertsResponse
    {
        public int TotalActiveAlerts { get; set; }

        public List<string> Alerts { get; set; } = new();
    }
}
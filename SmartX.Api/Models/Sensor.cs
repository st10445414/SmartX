namespace SmartX.Api.Models
{
    public class Sensor
    {
        public int Id { get; set; }

        public bool HasActiveAnomaly { get; set; }

        public string MacAddress { get; set; }
            = string.Empty;

        public string DeviceName { get; set; }
            = string.Empty;

        public string DeploymentLocation { get; set; }
            = string.Empty;

        public string Zone { get; set; }
            = string.Empty;

        public string Category { get; set; }
            = string.Empty;

        public DateTime DateRegistered { get; set; }
            = DateTime.UtcNow;

        public DateTime LastSeen { get; set; }
            = DateTime.UtcNow;

        public SensorStatus Status { get; set; }
            = SensorStatus.Connected;

        public SensorStatus CalculateStatus()
        {
            var minutesSinceLastSeen =
                (DateTime.UtcNow - LastSeen)
                .TotalMinutes;

            if (minutesSinceLastSeen > 15)
            {
                return SensorStatus.Disconnected;
            }

            if (HasActiveAnomaly ||
                minutesSinceLastSeen > 5)
            {
                return SensorStatus.Warning;
            }

            return SensorStatus.Connected;
        }
    }

    public enum SensorStatus
    {
        Connected,
        Warning,
        Disconnected
    }
}
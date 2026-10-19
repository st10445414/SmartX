namespace SmartX.Client.Models
{
    public class CommandStreamViewModel
    {
        public bool ApiOnline { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public List<SensorViewModel> Sensors { get; set; } = new();

        public DeviceCommandInput NewCommand { get; set; } = new();

        public List<string> ActiveAlerts { get; set; } = new();

        public CommandStatusViewModel CommandStatus { get; set; }
            = new();

        public int TotalSensors => Sensors.Count;

        public int ConnectedSensors =>
            Sensors.Count(s => s.Status == "Connected");

        public int WarningSensors =>
            Sensors.Count(s => s.Status == "Warning");

        public int DisconnectedSensors =>
            Sensors.Count(s => s.Status == "Disconnected");

        public List<SuggestedActionViewModel> Suggestions
        { get; set; } = new();

        public string SearchTerm { get; set; } = string.Empty;

        public string SelectedCategory { get; set; } = string.Empty;

        public string SelectedAlertFilter { get; set; } = "All";

        public List<string> AvailableCategories { get; set; } = new();


    }
}

namespace SmartX.Client.Models
{
    public class DashboardViewModel
    {
        public int TotalSensors { get; set; }

        public int ConnectedSensors { get; set; }

        public int WarningSensors { get; set; }

        public int DisconnectedSensors { get; set; }

        public List<SensorViewModel> Sensors { get; set; }
            = new List<SensorViewModel>();
    }
}
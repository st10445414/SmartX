using System.ComponentModel.DataAnnotations;

namespace SmartX.Client.Models
{
    public class SimulationViewModel
    {
        [Required]
        public int SensorId { get; set; }

        [Required]
        public string MetricName { get; set; }
            = "Temperature";

        [Required]
        public string Unit { get; set; }
            = "C";

        [Range(1, 10000)]
        public int BatchSize { get; set; }
            = 1000;

        public double MinimumValue { get; set; }
            = 18;

        public double MaximumValue { get; set; }
            = 35;

        public int Generated { get; set; }

        public int Received { get; set; }

        public int Stored { get; set; }

        public int AnomaliesDetected { get; set; }

        public double GenerationTimeMs { get; set; }

        public double ProcessingTimeMs { get; set; }

        public double TotalTimeMs { get; set; }

        public double? LastReading { get; set; }

        public bool ApiOnline { get; set; }

        public string ApiStatus { get; set; }
            = "Unknown";

        public List<SensorViewModel> Sensors { get; set; }
            = new();
    }
}

using System.ComponentModel.DataAnnotations;

namespace SmartX.Client.Models
{
    public class DeviceCommandInput
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a sensor.")]
        public int SensorId { get; set; }

        [Required]
        public string Action { get; set; } = "Pump";

        [Required]
        public string Value { get; set; } = "ON";

        public bool IsCritical { get; set; }
    }
}

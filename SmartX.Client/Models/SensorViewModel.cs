using System.ComponentModel.DataAnnotations;

namespace SmartX.Client.Models
{
    public class SensorViewModel
    {
        public int Id { get; set; }

        public bool HasActiveAnomaly { get; set; }

        [Required(ErrorMessage = "Device name is required.")]
        [Display(Name = "Device Name")]
        public string DeviceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "MAC address is required.")]
        [Display(Name = "MAC Address")]
        [RegularExpression(
            @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$",
            ErrorMessage =
                "Enter a valid MAC address, for example AA:BB:CC:DD:EE:FF.")]
        public string MacAddress { get; set; } = string.Empty;

        [Required(ErrorMessage =
            "Deployment location is required.")]
        [Display(Name = "Deployment Location")]
        public string DeploymentLocation { get; set; } =
            string.Empty;

        [Required(ErrorMessage = "Zone is required.")]
        public string Zone { get; set; } = string.Empty;

        [Required(ErrorMessage =
            "Sensor category is required.")]
        [Display(Name = "Sensor Category")]
        public string Category { get; set; } = string.Empty;

        public DateTime DateRegistered { get; set; }

        public DateTime LastSeen { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
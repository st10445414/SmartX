using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.Models
{
    public class RegisterSensorRequest
    {
        [Required]
        [StringLength(50)]
        public string DeviceName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$",
            ErrorMessage = "Enter a valid MAC address, for example AA:BB:CC:DD:EE:FF.")]
        public string MacAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DeploymentLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Zone { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;
    }
}
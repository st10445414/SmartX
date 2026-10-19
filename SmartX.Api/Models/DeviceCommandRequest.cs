using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.Models
{
    public class DeviceCommandRequest
    {
        [Range(1, int.MaxValue)]
        public int SensorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Value { get; set; } = string.Empty;

        public bool IsCritical { get; set; }
    }
}

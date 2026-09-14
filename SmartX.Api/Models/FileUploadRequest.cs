using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.Models
{
    public class FileUploadRequest
    {
        [Required]
        public int SensorId { get; set; }

        [Required]
        public IFormFile File { get; set; } = default!;

        public string Description { get; set; } = string.Empty;
    }
}
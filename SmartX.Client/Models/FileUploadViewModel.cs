using System.ComponentModel.DataAnnotations;

namespace SmartX.Client.Models
{
    public class FileUploadViewModel
    {
        [Required]
        public int SensorId { get; set; }

        [Required(
            ErrorMessage =
                "Please select a file.")]
        public IFormFile File { get; set; }
            = default!;

        [StringLength(200)]
        public string Description { get; set; }
            = string.Empty;
    }
}
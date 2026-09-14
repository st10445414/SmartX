namespace SmartX.Api.Models
{
    public class SensorAttachment
    {
        public int Id { get; set; }

        public int SensorId { get; set; }

        public string OriginalFileName { get; set; } = string.Empty;

        public string StoredFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public string Description { get; set; } = string.Empty;
    }
}
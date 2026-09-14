using SmartX.Api.Models;

namespace SmartX.Api.Repositories
{
    public class AttachmentRepository :
        IAttachmentRepository
    {
        private readonly List<SensorAttachment>
            _attachments = new();

        private int _nextId = 1;

        public Task<SensorAttachment> AddAsync(
            SensorAttachment attachment)
        {
            attachment.Id = _nextId++;

            _attachments.Add(attachment);

            return Task.FromResult(attachment);
        }

        public Task<List<SensorAttachment>>
            GetBySensorIdAsync(int sensorId)
        {
            var attachments =
                _attachments
                    .Where(a =>
                        a.SensorId == sensorId)
                    .OrderByDescending(a =>
                        a.UploadedAt)
                    .ToList();

            return Task.FromResult(attachments);
        }

        public Task<SensorAttachment?>
            GetByIdAsync(int id)
        {
            var attachment =
                _attachments
                    .FirstOrDefault(a =>
                        a.Id == id);

            return Task.FromResult(attachment);
        }
    }
}
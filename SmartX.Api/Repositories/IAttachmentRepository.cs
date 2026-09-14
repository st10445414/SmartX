using SmartX.Api.Models;

namespace SmartX.Api.Repositories
{
    public interface IAttachmentRepository
    {
        Task<SensorAttachment> AddAsync(
            SensorAttachment attachment);

        Task<List<SensorAttachment>> GetBySensorIdAsync(
            int sensorId);

        Task<SensorAttachment?> GetByIdAsync(
            int id);
    }
}
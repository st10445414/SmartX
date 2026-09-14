using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public interface IUploadApiService
    {
        Task<ApiResult<SensorAttachmentViewModel>>
            UploadAsync(
                FileUploadViewModel model);

        Task<List<SensorAttachmentViewModel>>
            GetSensorFilesAsync(
                int sensorId);
    }
}
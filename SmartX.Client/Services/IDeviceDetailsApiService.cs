
using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public interface IDeviceDetailsApiService
    {
        Task<DeviceDetailsViewModel?> GetDeviceDetailsAsync(
            string macAddress);

        Task<List<DeviceTelemetryViewModel>> GetTimelineAsync(
            int sensorId);
    }
}

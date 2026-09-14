using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public interface ISensorApiService
    {
        Task<List<SensorViewModel>> GetSensorsAsync();

        Task<ApiResult<SensorViewModel>> RegisterSensorAsync(
            SensorViewModel sensor);
    }
}
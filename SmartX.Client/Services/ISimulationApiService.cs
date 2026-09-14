using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public interface ISimulationApiService
    {
        Task<TelemetryBatchResultViewModel?>
            GenerateBatchAsync(
                SimulationViewModel model);

        Task<bool>
            CheckApiHealthAsync();
    }
}
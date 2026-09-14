using System.Net.Http.Json;
using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public class SimulationApiService :
        ISimulationApiService
    {
        private readonly HttpClient
            _httpClient;

        public SimulationApiService(
            HttpClient httpClient)
        {
            _httpClient =
                httpClient;
        }


        public async Task<
            TelemetryBatchResultViewModel?>
            GenerateBatchAsync(
                SimulationViewModel model)
        {
            try
            {
                var request =
                    new
                    {
                        sensorId =
                            model.SensorId,

                        metricName =
                            model.MetricName,

                        unit =
                            model.Unit,

                        batchSize =
                            model.BatchSize,

                        minimumValue =
                            model.MinimumValue,

                        maximumValue =
                            model.MaximumValue
                    };

                var response =
                    await _httpClient
                        .PostAsJsonAsync(
                            "api/Telemetry/batch",
                            request);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response
                    .Content
                    .ReadFromJsonAsync<
                        TelemetryBatchResultViewModel>();
            }
            catch
            {
                return null;
            }
        }


        public async Task<bool>
            CheckApiHealthAsync()
        {
            try
            {
                var response =
                    await _httpClient
                        .GetAsync(
                            "api/Health");

                return response
                    .IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
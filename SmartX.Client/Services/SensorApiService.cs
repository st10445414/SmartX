using System.Net.Http.Json;
using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public class SensorApiService : ISensorApiService
    {
        private readonly HttpClient _httpClient;

        public SensorApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SensorViewModel>>
            GetSensorsAsync()
        {
            var sensors =
                await _httpClient.GetFromJsonAsync<
                    List<SensorViewModel>>(
                    "api/sensors");

            return sensors ?? new List<SensorViewModel>();
        }

        public async Task<ApiResult<SensorViewModel>>
            RegisterSensorAsync(SensorViewModel sensor)
        {
            try
            {
                var response =
                    await _httpClient.PostAsJsonAsync(
                        "api/sensors",
                        sensor);

                if (response.IsSuccessStatusCode)
                {
                    var createdSensor =
                        await response.Content
                            .ReadFromJsonAsync<SensorViewModel>();

                    return new ApiResult<SensorViewModel>
                    {
                        Success = true,
                        Data = createdSensor,
                        Message = "Sensor registered successfully."
                    };
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Conflict)
                {
                    return new ApiResult<SensorViewModel>
                    {
                        Success = false,
                        Message =
                            "A sensor with this MAC address already exists."
                    };
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.BadRequest)
                {
                    return new ApiResult<SensorViewModel>
                    {
                        Success = false,
                        Message =
                            "The sensor details are invalid. Please review the form."
                    };
                }

                return new ApiResult<SensorViewModel>
                {
                    Success = false,
                    Message =
                        "The sensor could not be registered."
                };
            }
            catch (HttpRequestException)
            {
                return new ApiResult<SensorViewModel>
                {
                    Success = false,
                    Message =
                        "The Smart-X API could not be reached."
                };
            }
        }
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public class DeviceDetailsApiService : IDeviceDetailsApiService
    {
        private readonly HttpClient _httpClient;

        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true
            };

        public DeviceDetailsApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DeviceDetailsViewModel?>
            GetDeviceDetailsAsync(string macAddress)
        {
            string encodedMac =
                Uri.EscapeDataString(macAddress);

            using var response = await _httpClient.GetAsync(
                $"api/DataStructures/device/{encodedMac}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync());

            JsonElement root = document.RootElement;

            // Support either a direct sensor object
            // or a response containing a "sensor" property.
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("sensor", out var sensorElement))
            {
                root = sensorElement;
            }

            var sensor = root.Deserialize<SensorViewModel>(
                JsonOptions);

            if (sensor == null)
            {
                return null;
            }

            return new DeviceDetailsViewModel
            {
                SensorId = sensor.Id,
                DeviceName = sensor.DeviceName,
                MacAddress = sensor.MacAddress,
                Category = sensor.Category ?? string.Empty,
                Status = sensor.Status ?? string.Empty,


            };
        }

        public async Task<List<DeviceTelemetryViewModel>>
            GetTimelineAsync(int sensorId)
        {
            using var response = await _httpClient.GetAsync(
                $"api/DataStructures/timeline/{sensorId}");

            response.EnsureSuccessStatusCode();

            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync());

            JsonElement root = document.RootElement;

            // Handle a direct array of telemetry records.
            if (root.ValueKind == JsonValueKind.Array)
            {
                return root.Deserialize<
                    List<DeviceTelemetryViewModel>>(JsonOptions)
                    ?? new();
            }

            // Handle a response wrapping the records.
            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (string propertyName in new[]
                {
                    "records",
                    "timeline",
                    "readings"
                })
                {
                    if (root.TryGetProperty(
                        propertyName, out var recordsElement) &&
                        recordsElement.ValueKind ==
                            JsonValueKind.Array)
                    {
                        return recordsElement.Deserialize<
                            List<DeviceTelemetryViewModel>>(
                                JsonOptions) ?? new();
                    }
                }
            }

            return new List<DeviceTelemetryViewModel>();
        }
    }
}

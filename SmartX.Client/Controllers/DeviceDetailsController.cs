
using Microsoft.AspNetCore.Mvc;
using SmartX.Client.Services;

namespace SmartX.Client.Controllers
{
    public class DeviceDetailsController : Controller
    {
        private readonly IDeviceDetailsApiService
            _deviceDetailsApiService;

        public DeviceDetailsController(
            IDeviceDetailsApiService deviceDetailsApiService)
        {
            _deviceDetailsApiService = deviceDetailsApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string macAddress)
        {
            if (string.IsNullOrWhiteSpace(macAddress))
            {
                return BadRequest(
                    "A MAC address is required.");
            }

            try
            {
                var model =
                    await _deviceDetailsApiService
                        .GetDeviceDetailsAsync(macAddress);

                if (model == null)
                {
                    return NotFound(
                        "The requested device was not found.");
                }

                model.TelemetryHistory =
                    await _deviceDetailsApiService
                        .GetTimelineAsync(model.SensorId);

                model.TelemetryHistory =
                    model.TelemetryHistory
                        .OrderByDescending(t => t.Timestamp)
                        .ToList();

                return View(model);
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    503,
                    "Unable to retrieve device information.");
            }
            catch (TaskCanceledException)
            {
                return StatusCode(
                    504,
                    "The Smart-X API request timed out.");
            }
        }
    }
}

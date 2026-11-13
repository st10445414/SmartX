using Microsoft.AspNetCore.Mvc;
using SmartX.Client.Models;
using SmartX.Client.Services;
using System.Text.Json;

namespace SmartX.Client.Controllers
{
    public class TelemetryHistoryController : Controller
    {
        private readonly ITelemetryHistoryApiService
            _historyApi;

        public TelemetryHistoryController(
            ITelemetryHistoryApiService historyApi)
        {
            _historyApi = historyApi;
        }

        // GET: /TelemetryHistory
        public async Task<IActionResult> Index(
            int? sensorId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var model = new TelemetryHistoryViewModel();

            // Display the search form when no sensor is selected.
            if (!sensorId.HasValue)
            {
                return View(model);
            }

            model.SensorId = sensorId.Value;
            model.StartDate =
                startDate ?? DateTime.Today.AddDays(-7);
            model.EndDate =
                endDate ?? DateTime.Today.AddDays(1);

            model.HasSearched = true;

            // Validate the sensor ID.
            if (model.SensorId <= 0)
            {
                model.ErrorMessage =
                    "Please enter a valid sensor ID.";

                return View(model);
            }

            // Validate the date range.
            if (model.StartDate > model.EndDate)
            {
                model.ErrorMessage =
                    "Start date cannot be after end date.";

                return View(model);
            }

            try
            {
                var response = await _historyApi
                    .GetHistoricalReadingsAsync(
                        model.SensorId,
                        model.StartDate,
                        model.EndDate);

                // Read the API's algorithm name.
                if (response.TryGetProperty(
                    "algorithm", out var algorithm))
                {
                    model.Algorithm =
                        algorithm.GetString() ?? "AVL Tree";
                }

                // Read the historical telemetry array.
                if (response.TryGetProperty(
                    "readings", out var readings) &&
                    readings.ValueKind ==
                        JsonValueKind.Array)
                {
                    model.Readings = readings
                        .EnumerateArray()
                        .Select(r => r.Clone())
                        .ToList();
                }
            }
            catch (HttpRequestException ex)
            {
                model.ErrorMessage =
                    "Unable to retrieve historical telemetry. " +
                    "Please check that the Smart-X API " +
                    "is running.";

                // Technical details should be logged
                // rather than displayed to users.
            }
            catch (JsonException)
            {
                model.ErrorMessage =
                    "The API returned an unexpected " +
                    "telemetry response.";
            }

            return View(model);
        }
    }
}

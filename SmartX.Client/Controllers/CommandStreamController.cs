using Microsoft.AspNetCore.Mvc;
using SmartX.Client.Models;
using SmartX.Client.Services;

namespace SmartX.Client.Controllers
{
    public class CommandStreamController : Controller
    {
        private readonly ISensorApiService _sensorApiService;
        private readonly ICommandApiService _commandApiService;
        private readonly IInsightsApiService _insightsApiService;
        private readonly IActiveAlertApiService _activeAlertApiService;

        public CommandStreamController(
            ISensorApiService sensorApiService,
            ICommandApiService commandApiService,
            IInsightsApiService insightsApiService,
            IActiveAlertApiService activeAlertApiService)
        {
            _sensorApiService = sensorApiService;
            _commandApiService = commandApiService;
            _insightsApiService = insightsApiService;
            _activeAlertApiService = activeAlertApiService;
        }



        [HttpGet]

        [HttpGet]
        public async Task<IActionResult> Index(
                string? searchTerm,
                string? category,
                string? alertFilter)
        {
            var model = new CommandStreamViewModel
            {
                SearchTerm = searchTerm ?? string.Empty,
                SelectedCategory = category ?? string.Empty,
                SelectedAlertFilter = alertFilter ?? "All"
            };

            try
            {
                model.Sensors =
                    await _sensorApiService.GetSensorsAsync();

                // Build category options before filtering.
                model.AvailableCategories = model.Sensors
                    .Select(s => s.Category)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(c => c)
                    .ToList();

                var status =
                    await _commandApiService.GetStatusAsync();

                model.ApiOnline = status != null;

                if (status != null)
                {
                    model.CommandStatus = status;
                }

                model.Suggestions =
                    await _insightsApiService.GetSuggestionsAsync();

                var activeAlerts =
                    await _activeAlertApiService.GetAlertsAsync();

                model.ActiveAlerts =
                    activeAlerts?.Alerts ?? new List<string>();

                // Apply all three filters to the same collection.
                var filteredSensors = model.Sensors.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    string term = searchTerm.Trim();

                    filteredSensors = filteredSensors.Where(s =>
                        (s.DeviceName?.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase) ?? false)
                        ||
                        (s.MacAddress?.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase) ?? false)
                        ||
                        (s.Category?.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase) ?? false));
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    filteredSensors = filteredSensors.Where(s =>
                        string.Equals(
                            s.Category,
                            category,
                            StringComparison.OrdinalIgnoreCase));
                }

                if (string.Equals(
                    alertFilter, "Active",
                    StringComparison.OrdinalIgnoreCase))
                {
                    filteredSensors = filteredSensors.Where(s =>
                        model.ActiveAlerts.Any(alert =>
                            alert.StartsWith(
                                $"{s.Id}:",
                                StringComparison.OrdinalIgnoreCase)));
                }
                else if (string.Equals(
                    alertFilter, "None",
                    StringComparison.OrdinalIgnoreCase))
                {
                    filteredSensors = filteredSensors.Where(s =>
                        !model.ActiveAlerts.Any(alert =>
                            alert.StartsWith(
                                $"{s.Id}:",
                                StringComparison.OrdinalIgnoreCase)));
                }

                model.Sensors = filteredSensors.ToList();

                // Preserve the operator-search activity tracking.
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    foreach (var sensor in model.Sensors)
                    {
                        await _insightsApiService.RecordActivityAsync(
                            sensor.Id,
                            "Search",
                            searchTerm.Trim());
                    }
                }
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to retrieve live Smart-X information.";
            }
            catch (TaskCanceledException)
            {
                model.ErrorMessage =
                    "The Smart-X API request timed out.";
            }

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enqueue(
            [Bind(Prefix = "NewCommand")] DeviceCommandInput input)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Please select a sensor and provide valid command details.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _commandApiService.EnqueueAsync(input);


            if (result.Success)
            {
                try
                {
                    await _insightsApiService.RecordActivityAsync(
                        input.SensorId,
                        "Command",
                        $"{input.Action} {input.Value}");
                }
                catch (HttpRequestException)
                {
                    // The command was queued successfully.
                    // Activity recording can be retried separately.
                }
                catch (TaskCanceledException)
                {
                    // Avoids reporting the successful command as failed.
                }
            }


            TempData[result.Success ? "Success" : "Error"] =
                result.Message;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessNext()
        {
            var result = await _commandApiService.ProcessNextAsync();

            TempData[result.Success ? "Success" : "Error"] =
                result.Message;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Undo()
        {
            var result = await _commandApiService.UndoAsync();

            TempData[result.Success ? "Success" : "Error"] =
                result.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}

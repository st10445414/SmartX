using Microsoft.AspNetCore.Mvc;
using SmartX.Client.Models;
using SmartX.Client.Services;

namespace SmartX.Client.Controllers
{
    public class DashboardController :
        Controller
    {
        private readonly ISensorApiService
            _sensorApiService;

        public DashboardController(
            ISensorApiService sensorApiService)
        {
            _sensorApiService =
                sensorApiService;
        }

        public async Task<IActionResult> Index()
        {
            var sensors =
                await _sensorApiService
                    .GetSensorsAsync();

            sensors =
    sensors
        .OrderByDescending(
            s => s.Status ==
                 "Disconnected")
        .ThenByDescending(
            s => s.Status ==
                 "Warning")
        .ThenBy(
            s => s.DeviceName)
        .ToList();

            var model =
                new DashboardViewModel
                {
                    TotalSensors =
                        sensors.Count,

                    ConnectedSensors =
                        sensors.Count(
                            s =>
                                s.Status ==
                                "Connected"),

                    WarningSensors =
                        sensors.Count(
                            s =>
                                s.Status ==
                                "Warning"),

                    DisconnectedSensors =
                        sensors.Count(
                            s =>
                                s.Status ==
                                "Disconnected"),

                    Sensors =
                        sensors
                };

            return View(model);
        }
    }
}
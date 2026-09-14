using Microsoft.AspNetCore.Mvc;
using SmartX.Client.Models;
using SmartX.Client.Services;

namespace SmartX.Client.Controllers
{
    public class SimulationController :
        Controller
    {
        private readonly
            ISimulationApiService
            _simulationApiService;

        private readonly
            ISensorApiService
            _sensorApiService;


        public SimulationController(
            ISimulationApiService
                simulationApiService,

            ISensorApiService
                sensorApiService)
        {
            _simulationApiService =
                simulationApiService;

            _sensorApiService =
                sensorApiService;
        }


        [HttpGet]
        public async Task<IActionResult>
            Index()
        {
            var sensors =
                await _sensorApiService
                    .GetSensorsAsync();

            var apiOnline =
                await _simulationApiService
                    .CheckApiHealthAsync();

            var model =
                new SimulationViewModel
                {
                    Sensors =
                        sensors,

                    ApiOnline =
                        apiOnline,

                    ApiStatus =
                        apiOnline
                            ? "Online"
                            : "Offline"
                };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult>
            Generate(
                SimulationViewModel model)
        {
            model.Sensors =
                await _sensorApiService
                    .GetSensorsAsync();

            model.ApiOnline =
                await _simulationApiService
                    .CheckApiHealthAsync();

            model.ApiStatus =
                model.ApiOnline
                    ? "Online"
                    : "Offline";

            if (!ModelState.IsValid)
            {
                return View(
                    "Index",
                    model);
            }

            var result =
                await _simulationApiService
                    .GenerateBatchAsync(
                        model);

            if (result == null)
            {
                ModelState.AddModelError(
                    "",
                    "Telemetry batch could not be generated.");

                return View(
                    "Index",
                    model);
            }

            model.Generated =
                result.Generated;

            model.Received =
                result.Received;

            model.Stored =
                result.Stored;

            model.AnomaliesDetected =
                result.AnomaliesDetected;

            model.GenerationTimeMs =
                result.GenerationTimeMs;

            model.ProcessingTimeMs =
                result.ProcessingTimeMs;

            model.TotalTimeMs =
                result.TotalTimeMs;

            model.LastReading =
                result.LastReading;

            return View(
                "Index",
                model);
        }
    }
}
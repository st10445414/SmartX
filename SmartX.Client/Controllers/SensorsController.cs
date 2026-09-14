using Microsoft.AspNetCore.Mvc;
using SmartX.Client.Models;
using SmartX.Client.Services;

namespace SmartX.Client.Controllers
{
    public class SensorsController : Controller
    {
        private readonly ISensorApiService _sensorApiService;

        private readonly IUploadApiService _uploadApiService;

        public SensorsController(
            ISensorApiService sensorApiService,
            IUploadApiService uploadApiService)
        {
            _sensorApiService =
                sensorApiService;

            _uploadApiService =
                uploadApiService;
        }

        public async Task<IActionResult> Index()
        {
            var sensors =
                await _sensorApiService
                    .GetSensorsAsync();

            return View(sensors);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(
                new SensorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            SensorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result =
                await _sensorApiService
                    .RegisterSensorAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message);

                return View(model);
            }

            TempData["SuccessMessage"] =
                result.Message;

            return RedirectToAction(
                nameof(Index));
        }

        [HttpGet]
        public IActionResult Upload(
    int sensorId)
        {
            var model =
                new FileUploadViewModel
                {
                    SensorId = sensorId
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(
             FileUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result =
                await _uploadApiService
                    .UploadAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message);

                return View(model);
            }

            TempData["SuccessMessage"] =
                result.Message;

            return RedirectToAction(
                nameof(Index));
        }

    }
}
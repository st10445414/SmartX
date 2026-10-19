
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataStructuresController : ControllerBase
    {
        private readonly DeviceLookupService _deviceLookup;
        private readonly TelemetryTimelineService _timeline;
        private readonly UniqueAlertService _alerts;

        public DataStructuresController(
            DeviceLookupService deviceLookup,
            TelemetryTimelineService timeline,
            UniqueAlertService alerts)
        {
            _deviceLookup = deviceLookup;
            _timeline = timeline;
            _alerts = alerts;
        }

        [HttpGet("device/{macAddress}")]
        public async Task<IActionResult> FindDevice(string macAddress)
        {
            var sensor = await _deviceLookup.FindByMacAsync(macAddress);

            if (sensor == null)
                return NotFound(new { message = "Device not found." });

            return Ok(sensor);
        }

        [HttpPost("refresh-device-cache")]
        public async Task<IActionResult> RefreshCache()
        {
            int count = await _deviceLookup.RefreshCacheAsync();

            return Ok(new
            {
                message = "Device cache refreshed.",
                cachedDevices = count
            });
        }

        [HttpGet("timeline/{sensorId:int}")]
        public async Task<IActionResult> GetTimeline(int sensorId)
        {
            return Ok(await _timeline.GetTimelineAsync(sensorId));
        }

        [HttpPost("alerts/{sensorId:int}/{alertType}")]
        public IActionResult RegisterAlert(int sensorId, string alertType)
        {
            bool added = _alerts.RegisterAlert(sensorId, alertType);

            return Ok(new
            {
                sensorId,
                alertType,
                added,
                message = added
                    ? "New alert registered."
                    : "Duplicate alert suppressed."
            });
        }

        [HttpDelete("alerts/{sensorId:int}/{alertType}")]
        public IActionResult ResolveAlert(int sensorId, string alertType)
        {
            bool removed = _alerts.ResolveAlert(sensorId, alertType);

            return Ok(new
            {
                sensorId,
                alertType,
                removed
            });
        }

        [HttpGet("alerts")]
        public IActionResult GetAlerts()
        {
            var alerts = _alerts.GetActiveAlerts();

            return Ok(new
            {
                totalActiveAlerts = alerts.Length,
                alerts
            });
        }
    }
}

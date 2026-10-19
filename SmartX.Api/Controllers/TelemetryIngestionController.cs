using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Repositories;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryIngestionController : ControllerBase
    {
        private readonly TelemetryQueueService _queue;
        private readonly ISensorRepository _sensors;

        public TelemetryIngestionController(
            TelemetryQueueService queue,
            ISensorRepository sensors)
        {
            _queue = queue;
            _sensors = sensors;
        }

        [HttpPost]
        public async Task<IActionResult> Ingest(
            [FromBody] TelemetryIngestionRequest request)
        {
            if (!double.IsFinite(request.Value))
            {
                return BadRequest(new
                {
                    message = "Telemetry value must be finite."
                });
            }

            var sensor = await _sensors.GetByIdAsync(request.SensorId);

            if (sensor == null)
            {
                return NotFound(new
                {
                    message = "Registered sensor not found."
                });
            }

            _queue.Enqueue(request);

            return Accepted(new
            {
                message = "Telemetry accepted for processing.",
                sensorId = request.SensorId,
                metricName = request.MetricName,
                status = "Queued"
            });
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(_queue.GetStatus());
        }
    }
}

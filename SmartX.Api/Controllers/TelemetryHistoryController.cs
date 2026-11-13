
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryHistoryController : ControllerBase
    {
        private readonly TelemetryAvlService _avlService;

        public TelemetryHistoryController(
            TelemetryAvlService avlService)
        {
            _avlService = avlService;
        }

        // GET: api/TelemetryHistory/sensor/1
        [HttpGet("sensor/{sensorId:int}")]
        public async Task<IActionResult> GetHistory(
            int sensorId,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            if (sensorId <= 0)
            {
                return BadRequest(new
                {
                    Message = "A valid sensor ID is required."
                });
            }

            if (start == default || end == default)
            {
                return BadRequest(new
                {
                    Message = "Both start and end dates are required."
                });
            }

            if (start > end)
            {
                return BadRequest(new
                {
                    Message = "Start date cannot be after end date."
                });
            }

            try
            {
                var readings = await _avlService
                    .SearchHistoricalReadingsAsync(
                        sensorId, start, end);

                return Ok(new
                {
                    SensorId = sensorId,
                    StartDate = start,
                    EndDate = end,
                    Algorithm = "AVL Tree",
                    TotalReadings = readings.Count,
                    Readings = readings
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }
    }
}

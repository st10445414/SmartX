
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsightsController : ControllerBase
    {
        private readonly PredictiveActionService _predictionService;

        public InsightsController(
            PredictiveActionService predictionService)
        {
            _predictionService = predictionService;
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> Suggestions()
        {
            var suggestions =
                await _predictionService.GetSuggestionsAsync();

            return Ok(new
            {
                count = suggestions.Count,
                suggestions
            });
        }

        [HttpPost("activity")]
        public IActionResult RecordActivity(
            [FromBody] OperatorActivity activity)
        {
            if (activity.SensorId <= 0 ||
                !new[] { "Search", "Command" }
                    .Contains(activity.ActivityType))
            {
                return BadRequest(new
                {
                    message = "Invalid activity."
                });
            }

            _predictionService.RecordActivity(activity);

            return Accepted(new
            {
                message = "Activity recorded."
            });
        }
    }
}

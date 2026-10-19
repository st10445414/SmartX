using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Repositories;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommandsController : ControllerBase
    {
        private readonly CommandProcessingService _commandService;
        private readonly ISensorRepository _sensorRepository;

        public CommandsController(
            CommandProcessingService commandService,
            ISensorRepository sensorRepository)
        {
            _commandService = commandService;
            _sensorRepository = sensorRepository;
        }

        [HttpPost("enqueue")]
        public async Task<IActionResult> Enqueue(
            [FromBody] DeviceCommandRequest request)
        {
            var sensor = await _sensorRepository
                .GetByIdAsync(request.SensorId);

            if (sensor == null)
            {
                return NotFound(new
                {
                    message = "Sensor not found."
                });
            }

            var command = new DeviceCommand
            {
                SensorId = request.SensorId,
                Action = request.Action,
                Value = request.Value,
                IsCritical = request.IsCritical
            };

            _commandService.Enqueue(command);

            return Accepted(command);
        }

        [HttpPost("process-next")]
        public IActionResult ProcessNext()
        {
            var command = _commandService.ProcessNext();

            if (command == null)
            {
                return Ok(new
                {
                    message = "No commands are waiting."
                });
            }

            return Ok(command);
        }

        [HttpPost("undo")]
        public IActionResult Undo()
        {
            var entry = _commandService.UndoLast();

            if (entry == null)
            {
                return BadRequest(new
                {
                    message = "No commands available to undo."
                });
            }

            return Ok(new
            {
                message = "Previous device state restored.",
                entry
            });
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(_commandService.GetStatus());
        }
    }
}

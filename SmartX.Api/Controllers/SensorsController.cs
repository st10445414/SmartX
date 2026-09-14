using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorsController : ControllerBase
    {
        private readonly ISensorRepository _sensorRepository;

        public SensorsController(ISensorRepository sensorRepository)
        {
            _sensorRepository = sensorRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sensor>>> GetAllSensors()
        {
            var sensors = await _sensorRepository.GetAllAsync();

            return Ok(sensors);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Sensor>> GetSensorById(int id)
        {
            var sensor = await _sensorRepository.GetByIdAsync(id);

            if (sensor == null)
            {
                return NotFound(new
                {
                    message = $"Sensor with ID {id} was not found."
                });
            }

            return Ok(sensor);
        }

        [HttpPost]
        public async Task<ActionResult<Sensor>> RegisterSensor(
            RegisterSensorRequest request)
        {
            var existingSensor =
                await _sensorRepository.GetByMacAddressAsync(
                    request.MacAddress);

            if (existingSensor != null)
            {
                return Conflict(new
                {
                    message =
                        "A sensor with this MAC address is already registered."
                });
            }

            var sensor = new Sensor
            {
                DeviceName = request.DeviceName,
                MacAddress = request.MacAddress.ToUpperInvariant(),
                DeploymentLocation = request.DeploymentLocation,
                Zone = request.Zone,
                Category = request.Category,
                DateRegistered = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                Status = SensorStatus.Connected
            };

            var createdSensor =
                await _sensorRepository.AddAsync(sensor);

            return CreatedAtAction(
                nameof(GetSensorById),
                new { id = createdSensor.Id },
                createdSensor);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSensor(int id)
        {
            var deleted =
                await _sensorRepository.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = $"Sensor with ID {id} was not found."
                });
            }

            return NoContent();
        }
    }
}
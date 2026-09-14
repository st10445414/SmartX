using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Repositories;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController :
        ControllerBase
    {
        private readonly TelemetryHistoryService
            _historyService;

        private readonly ITelemetryRepository
            _telemetryRepository;

        private readonly ISensorRepository
            _sensorRepository;

        private readonly TelemetryValidationService
            _validationService;

        private readonly TypedTelemetryStore
            _typedTelemetryStore;

        public TelemetryController(
            ITelemetryRepository telemetryRepository,
            ISensorRepository sensorRepository,
            TelemetryValidationService validationService,
            TelemetryHistoryService historyService,
            TypedTelemetryStore typedTelemetryStore)
        {
            _telemetryRepository =
                telemetryRepository;

            _sensorRepository =
                sensorRepository;

            _validationService =
                validationService;

            _historyService =
                historyService;

            _typedTelemetryStore =
                typedTelemetryStore;
        }

        [HttpPost("actuator")]
        public async Task<IActionResult>
    SubmitActuatorTelemetry(
        ActuatorTelemetryRequest request)
        {
            var sensor =
                await _sensorRepository
                    .GetByIdAsync(
                        request.SensorId);

            if (sensor == null)
            {
                return NotFound(new
                {
                    message =
                        "Sensor could not be found."
                });
            }

            var packet =
                new TelemetryPacket<bool>
                {
                    SensorId =
                        request.SensorId,

                    MetricName =
                        request.MetricName,

                    Value =
                        request.Value,

                    Unit =
                        "Boolean",

                    Timestamp =
                        request.Timestamp
                };

            _typedTelemetryStore.Add(packet);

            sensor.LastSeen =
                DateTime.UtcNow;

            sensor.Status =
                sensor.CalculateStatus();

            return Ok(packet);
        }

        [HttpGet("typed-summary")]
        public IActionResult
    GetTypedTelemetrySummary()
        {
            return Ok(new
            {
                decimalReadings =
                    _typedTelemetryStore
                        .GetDecimalTelemetry(),

                integerReadings =
                    _typedTelemetryStore
                        .GetIntegerTelemetry(),

                booleanReadings =
                    _typedTelemetryStore
                        .GetBooleanTelemetry(),

                decimalCount =
                    _typedTelemetryStore
                        .GetDecimalTelemetry()
                        .Count,

                integerCount =
                    _typedTelemetryStore
                        .GetIntegerTelemetry()
                        .Count,

                booleanCount =
                    _typedTelemetryStore
                        .GetBooleanTelemetry()
                        .Count
            });
        }

        [HttpGet("aggregation-demo")]
        public IActionResult
    GetAggregationDemo()
        {
            var meterOne =
                new NumericSensorReading<int>(
                    450,
                    "Power",
                    "W");

            var meterTwo =
                new NumericSensorReading<int>(
                    350,
                    "Power",
                    "W");

            var combined =
                meterOne + meterTwo;

            var difference =
                meterOne - meterTwo;

            var exceedsThreshold =
                combined > 750;

            return Ok(new
            {
                meterOne =
                    meterOne.Value,

                meterTwo =
                    meterTwo.Value,

                combinedPower =
                    combined.Value,

                difference =
                    difference.Value,

                unit =
                    combined.Unit,

                exceeds750Watts =
                    exceedsThreshold
            });
        }

        [HttpPost("power")]
        public async Task<IActionResult>
    SubmitPowerTelemetry(
        PowerTelemetryRequest request)
        {
            var sensor =
                await _sensorRepository
                    .GetByIdAsync(
                        request.SensorId);

            if (sensor == null)
            {
                return NotFound(new
                {
                    message =
                        "Sensor could not be found."
                });
            }

            var packet =
                new TelemetryPacket<int>
                {
                    SensorId =
                        request.SensorId,

                    MetricName =
                        request.MetricName,

                    Value =
                        request.Value,

                    Unit =
                        request.Unit,

                    Timestamp =
                        request.Timestamp
                };

            _typedTelemetryStore.Add(packet);

            sensor.LastSeen =
                DateTime.UtcNow;

            sensor.HasActiveAnomaly =
                request.Value > 5000 ||
                request.Value < 0;

            sensor.Status =
                sensor.CalculateStatus();

            return Ok(packet);
        }

        [HttpPost]
        public async Task<
            ActionResult<TelemetryRecord>>
            SubmitTelemetry(
                TelemetryRequest request)
        {
            var sensor =
                await _sensorRepository
                    .GetByIdAsync(
                        request.SensorId);

            if (sensor == null)
            {
                return NotFound(
                    new
                    {
                        message =
                            "Sensor could not be found."
                    });
            }

            var packet =
                new TelemetryPacket<double>
                {
                    SensorId =
                        request.SensorId,

                    MetricName =
                        request.MetricName,

                    Value =
                        request.Value,

                    Unit =
                        request.Unit,

                    Timestamp =
                        request.Timestamp
                };

            _typedTelemetryStore.Add(packet);

            var validatedRecord =
                _validationService
                    .Validate(packet);

            await _telemetryRepository
                .AddAsync(validatedRecord);

            sensor.LastSeen =
                DateTime.UtcNow;

            sensor.HasActiveAnomaly =
                validatedRecord.IsAnomaly;

            sensor.Status =
                sensor.CalculateStatus();

            return Ok(validatedRecord);
        }

        [HttpGet]
        public async Task<
            ActionResult<
                List<TelemetryRecord>>>
            GetAllTelemetry()
        {
            var records =
                await _telemetryRepository
                    .GetAllAsync();

            return Ok(records);
        }

        [HttpGet("sensor/{sensorId:int}")]
        public async Task<
            ActionResult<
                List<TelemetryRecord>>>
            GetSensorTelemetry(
                int sensorId)
        {
            var records =
                await _telemetryRepository
                    .GetBySensorIdAsync(
                        sensorId);

            return Ok(records);
        }
        
        [HttpGet("history")]
        public IActionResult
        GetHistoricalTelemetry()
        {
            var history =
                _historyService
                    .GetRawHistory();

            var isValid =
                _historyService
                    .ValidateHistoryRecursive(
                        history);

            var list =
                _historyService
                    .ConvertToOptimisedList();

            return Ok(
                new
                {
                    isValid,
                    rawHistory = history,
                    optimisedList = list,
                    average =
                        _historyService
                            .GetAverage(),
                    maximum =
                        _historyService
                            .GetMaximum()
                });
        }
    }
}
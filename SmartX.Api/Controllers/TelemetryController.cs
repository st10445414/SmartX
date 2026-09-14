using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Repositories;
using SmartX.Api.Services;
using System.Diagnostics;

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

        [HttpGet("search")]
        public async Task<IActionResult> SearchTelemetry(
    int sensorId,
    string? metricName = null)
        {
            var stopwatch =
                Stopwatch.StartNew();

            var records =
                await _telemetryRepository
                    .GetBySensorIdAsync(sensorId);

            var filteredRecords =
                records.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(
                    metricName))
            {
                filteredRecords =
                    filteredRecords.Where(
                        r =>
                            r.MetricName.Equals(
                                metricName,
                                StringComparison
                                    .OrdinalIgnoreCase));
            }

            var results =
                filteredRecords
                    .OrderByDescending(
                        r => r.Timestamp)
                    .ToList();

            stopwatch.Stop();

            return Ok(new
            {
                sensorId,

                metricName =
                    metricName ??
                    "All",

                resultCount =
                    results.Count,

                searchTimeMs =
                    stopwatch.Elapsed
                        .TotalMilliseconds,

                records =
                    results
            });
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

        [HttpPost("batch")]
        public async Task<IActionResult> SubmitBatchTelemetry(
    TelemetryBatchRequest request)
        {
            if (request.BatchSize < 1 ||
                request.BatchSize > 10000)
            {
                return BadRequest(new
                {
                    message =
                        "Batch size must be between 1 and 10,000."
                });
            }

            if (request.MaximumValue <=
                request.MinimumValue)
            {
                return BadRequest(new
                {
                    message =
                        "Maximum value must be greater than minimum value."
                });
            }

            var sensor =
                await _sensorRepository
                    .GetByIdAsync(request.SensorId);

            if (sensor == null)
            {
                return NotFound(new
                {
                    message =
                        "Sensor could not be found."
                });
            }

            var totalStopwatch =
                Stopwatch.StartNew();

            var generationStopwatch =
                Stopwatch.StartNew();

            var random =
                new Random();

            var packets =
                new List<TelemetryPacket<double>>(
                    request.BatchSize);

            for (int i = 0;
                 i < request.BatchSize;
                 i++)
            {
                var value =
                    request.MinimumValue +
                    random.NextDouble() *
                    (request.MaximumValue -
                     request.MinimumValue);

                var packet =
                    new TelemetryPacket<double>
                    {
                        SensorId =
                            request.SensorId,

                        MetricName =
                            request.MetricName,

                        Value =
                            Math.Round(value, 2),

                        Unit =
                            request.Unit,

                        Timestamp =
                            DateTime.UtcNow
                                .AddMilliseconds(i)
                    };

                packets.Add(packet);
            }

            generationStopwatch.Stop();


            var processingStopwatch =
                Stopwatch.StartNew();

            int stored = 0;
            int anomalies = 0;

            foreach (var packet in packets)
            {
                _typedTelemetryStore.Add(packet);

                var record =
                    _validationService
                        .Validate(packet);

                await _telemetryRepository
                    .AddAsync(record);

                stored++;

                if (record.IsAnomaly)
                {
                    anomalies++;
                }
            }

            processingStopwatch.Stop();

            sensor.LastSeen =
                DateTime.UtcNow;

            sensor.HasActiveAnomaly =
                anomalies > 0;

            sensor.Status =
                sensor.CalculateStatus();

            totalStopwatch.Stop();


            var result =
                new TelemetryBatchResult
                {
                    Generated =
                        packets.Count,

                    Received =
                        packets.Count,

                    Stored =
                        stored,

                    AnomaliesDetected =
                        anomalies,

                    GenerationTimeMs =
                        generationStopwatch
                            .Elapsed
                            .TotalMilliseconds,

                    ProcessingTimeMs =
                        processingStopwatch
                            .Elapsed
                            .TotalMilliseconds,

                    TotalTimeMs =
                        totalStopwatch
                            .Elapsed
                            .TotalMilliseconds,

                    LastReading =
                        packets.LastOrDefault()?.Value,

                    MetricName =
                        request.MetricName,

                    Unit =
                        request.Unit
                };

            return Ok(result);
        }
    }
}
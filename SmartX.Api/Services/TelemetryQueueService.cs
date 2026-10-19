
using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Services
{
    public class TelemetryQueueService
    {

        private readonly Dictionary<string, DateTime>
            _latestAlertTimestamps = new();


        private readonly Dictionary<
            TelemetryIngestionRequest,
            TaskCompletionSource<TelemetryRecord>>
            _pendingCompletions = new();


        private readonly Queue<TelemetryIngestionRequest> _normalQueue = new();

        private readonly PriorityQueue<TelemetryIngestionRequest, long>
            _criticalQueue = new();

        private readonly ITelemetryRepository _telemetryRepository;
        private readonly UniqueAlertService _alertService;

        private readonly object _sync = new();
        private readonly SemaphoreSlim _signal = new(0);

        private long _sequence;
        private int _processedCount;
        private int _failedCount;

        public TelemetryQueueService(
            ITelemetryRepository telemetryRepository,
            UniqueAlertService alertService)
        {
            _telemetryRepository = telemetryRepository;
            _alertService = alertService;
        }


        public void Enqueue(TelemetryIngestionRequest reading)
        {
            EnqueueInternal(reading, null);
        }

        public Task<TelemetryRecord> EnqueueAndWaitAsync(
            TelemetryIngestionRequest reading)
        {
            var completion =
                new TaskCompletionSource<TelemetryRecord>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            EnqueueInternal(reading, completion);

            return completion.Task;
        }

        private void EnqueueInternal(
            TelemetryIngestionRequest reading,
            TaskCompletionSource<TelemetryRecord>? completion)
        {
            var packet = new TelemetryIngestionRequest
            {
                SensorId = reading.SensorId,
                MetricName = reading.MetricName.Trim(),
                Value = reading.Value,
                Unit = reading.Unit.Trim(),
                Timestamp = reading.Timestamp
            };

            lock (_sync)
            {
                if (completion != null)
                {
                    _pendingCompletions[packet] = completion;
                }

                if (IsCritical(packet))
                {
                    _criticalQueue.Enqueue(packet, _sequence++);
                }
                else
                {
                    _normalQueue.Enqueue(packet);
                }
            }

            _signal.Release();
        }


        public async Task WaitForWorkAsync(
            CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
        }


        public async Task<TelemetryRecord?> ProcessNextAsync()
        {
            TelemetryIngestionRequest? reading;
            TaskCompletionSource<TelemetryRecord>? completion;

            lock (_sync)
            {
                if (_criticalQueue.Count > 0)
                {
                    reading = _criticalQueue.Dequeue();
                }
                else if (_normalQueue.Count > 0)
                {
                    reading = _normalQueue.Dequeue();
                }
                else
                {
                    return null;
                }

                _pendingCompletions.Remove(
                    reading, out completion);
            }

            try
            {
                bool critical = IsCritical(reading);

                var record = new TelemetryRecord
                {
                    SensorId = reading.SensorId,
                    MetricName = reading.MetricName,
                    Value = reading.Value,
                    Unit = reading.Unit,
                    Timestamp = reading.Timestamp,
                    IsAnomaly = critical,
                    AnomalyReason = critical
                        ? "Reading outside expected threshold."
                        : string.Empty
                };

                await _telemetryRepository.AddAsync(record);

                string alertType =
                    reading.MetricName.Trim().ToUpperInvariant();

                string alertKey =
                    $"{reading.SensorId}:{alertType}";

                lock (_sync)
                {
                    bool isLatestReading =
                        !_latestAlertTimestamps.TryGetValue(
                            alertKey, out DateTime latestTimestamp)
                        || reading.Timestamp >= latestTimestamp;

                    if (isLatestReading)
                    {
                        _latestAlertTimestamps[alertKey] =
                            reading.Timestamp;

                        if (critical)
                        {
                            _alertService.RegisterAlert(
                                reading.SensorId, alertType);
                        }
                        else
                        {
                            _alertService.ResolveAlert(
                                reading.SensorId, alertType);
                        }
                    }
                }

                lock (_sync)
                {
                    _processedCount++;
                }

                completion?.TrySetResult(record);

                return record;
            }
            catch (Exception ex)
            {
                lock (_sync)
                {
                    _failedCount++;
                }

                completion?.TrySetException(ex);

                throw;
            }
        }


        public object GetStatus()
        {
            lock (_sync)
            {
                return new
                {
                    NormalQueueCount = _normalQueue.Count,
                    CriticalQueueCount = _criticalQueue.Count,
                    ProcessedCount = _processedCount,
                    FailedCount = _failedCount
                };
            }
        }

        private static bool IsCritical(TelemetryIngestionRequest reading)
        {
            string metric =
                reading.MetricName.Trim().ToLowerInvariant();

            return metric switch
            {
                "moisture" => reading.Value < 20,
                "temperature" => reading.Value > 40,
                "humidity" => reading.Value < 20 || reading.Value > 80,
                "power" => reading.Value > 5000,
                _ => false
            };
        }
    }
}

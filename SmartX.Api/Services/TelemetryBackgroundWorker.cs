
using Microsoft.Extensions.Hosting;

namespace SmartX.Api.Services
{
    public class TelemetryBackgroundWorker : BackgroundService
    {
        private readonly TelemetryQueueService _queue;
        private readonly ILogger<TelemetryBackgroundWorker> _logger;

        public TelemetryBackgroundWorker(
            TelemetryQueueService queue,
            ILogger<TelemetryBackgroundWorker> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Smart-X telemetry processor started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _queue.WaitForWorkAsync(stoppingToken);

                    var processed = await _queue.ProcessNextAsync();

                    if (processed != null)
                    {
                        _logger.LogInformation(
                            "Processed sensor {SensorId}, metric {Metric}",
                            processed.SensorId,
                            processed.MetricName);
                    }
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex, "Telemetry processing failed.");
                }
            }
        }
    }
}

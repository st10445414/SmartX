namespace SmartX.Api.Models
{
    public class TelemetryBatchResult
    {
        public int Generated { get; set; }

        public int Received { get; set; }

        public int Stored { get; set; }

        public int AnomaliesDetected { get; set; }

        public double GenerationTimeMs { get; set; }

        public double ProcessingTimeMs { get; set; }

        public double TotalTimeMs { get; set; }

        public double? LastReading { get; set; }

        public string MetricName { get; set; }
            = string.Empty;

        public string Unit { get; set; }
            = string.Empty;
    }
}
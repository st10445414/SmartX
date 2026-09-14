namespace SmartX.Api.Models
{
    public class SensorReading
    {
        public double Value { get; set; }

        public string MetricName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public SensorReading(double value, string metricName, string unit)
        {
            Value = value;
            MetricName = metricName;
            Unit = unit;
        }

        public static SensorReading operator +(
            SensorReading first,
            SensorReading second)
        {
            if (first.MetricName != second.MetricName ||
                first.Unit != second.Unit)
            {
                throw new InvalidOperationException(
                    "Only compatible sensor readings can be aggregated.");
            }

            return new SensorReading(
                first.Value + second.Value,
                first.MetricName,
                first.Unit);
        }

        public static SensorReading operator -(
            SensorReading first,
            SensorReading second)
        {
            if (first.MetricName != second.MetricName ||
                first.Unit != second.Unit)
            {
                throw new InvalidOperationException(
                    "Only compatible sensor readings can be compared.");
            }

            return new SensorReading(
                first.Value - second.Value,
                first.MetricName,
                first.Unit);
        }

        public static bool operator >(
            SensorReading reading,
            double threshold)
        {
            return reading.Value > threshold;
        }

        public static bool operator <(
            SensorReading reading,
            double threshold)
        {
            return reading.Value < threshold;
        }
    }
}
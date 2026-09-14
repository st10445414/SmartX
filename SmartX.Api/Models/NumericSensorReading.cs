using System.Numerics;

namespace SmartX.Api.Models
{
    public readonly record struct NumericSensorReading<T>
        where T : INumber<T>
    {
        public T Value { get; init; }

        public string MetricName { get; init; }

        public string Unit { get; init; }


        public NumericSensorReading(
            T value,
            string metricName,
            string unit)
        {
            Value = value;
            MetricName = metricName;
            Unit = unit;
        }


        private static void
            ValidateCompatibility(
                NumericSensorReading<T> first,
                NumericSensorReading<T> second)
        {
            if (!string.Equals(
                    first.MetricName,
                    second.MetricName,
                    StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(
                    first.Unit,
                    second.Unit,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Only compatible sensor readings can be combined.");
            }
        }


        public static NumericSensorReading<T>
            operator +(
                NumericSensorReading<T> first,
                NumericSensorReading<T> second)
        {
            ValidateCompatibility(
                first,
                second);

            return new NumericSensorReading<T>(
                first.Value + second.Value,
                first.MetricName,
                first.Unit);
        }


        public static NumericSensorReading<T>
            operator -(
                NumericSensorReading<T> first,
                NumericSensorReading<T> second)
        {
            ValidateCompatibility(
                first,
                second);

            return new NumericSensorReading<T>(
                first.Value - second.Value,
                first.MetricName,
                first.Unit);
        }


        public static bool operator >(
            NumericSensorReading<T> reading,
            T threshold)
        {
            return reading.Value >
                   threshold;
        }


        public static bool operator <(
            NumericSensorReading<T> reading,
            T threshold)
        {
            return reading.Value <
                   threshold;
        }
    }
}
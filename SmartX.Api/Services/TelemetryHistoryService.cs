using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class TelemetryHistoryService
    {
        private readonly double[][]
            _historicalTelemetry;

        public TelemetryHistoryService()
        {
            _historicalTelemetry =
                new double[4][];

            _historicalTelemetry[0] =
                new double[]
                {
                    22.5,
                    23.1,
                    24.0,
                    23.8
                };

            _historicalTelemetry[1] =
                new double[]
                {
                    55.0,
                    58.2,
                    57.7
                };

            _historicalTelemetry[2] =
                new double[]
                {
                    1008.5,
                    1009.1,
                    1007.8,
                    1006.9,
                    1007.4
                };

            _historicalTelemetry[3] =
                new double[]
                {
                    450,
                    620,
                    810
                };
        }

        public double[][] GetRawHistory()
        {
            return _historicalTelemetry;
        }

        public List<double>
    ConvertToOptimisedList()
        {
            var values =
                new List<double>();

            foreach (
                var sensorReadings
                in _historicalTelemetry)
            {
                values.AddRange(
                    sensorReadings);
            }

            return values;
        }

        public bool ValidateHistoryRecursive(
    double[][] history,
    int rowIndex = 0,
    int columnIndex = 0)
        {
            if (rowIndex >= history.Length)
            {
                return true;
            }

            if (columnIndex >=
                history[rowIndex].Length)
            {
                return ValidateHistoryRecursive(
                    history,
                    rowIndex + 1,
                    0);
            }

            var currentValue =
                history[rowIndex][columnIndex];

            if (double.IsNaN(currentValue) ||
                double.IsInfinity(currentValue))
            {
                return false;
            }

            return ValidateHistoryRecursive(
                history,
                rowIndex,
                columnIndex + 1);
        }

        public double GetAverage()
        {
            var values =
                ConvertToOptimisedList();

            return values.Count == 0
                ? 0
                : values.Average();
        }

        public double GetMaximum()
        {
            var values =
                ConvertToOptimisedList();

            return values.Count == 0
                ? 0
                : values.Max();
        }

    }
}
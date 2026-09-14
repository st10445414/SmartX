using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class TelemetryValidationService
    {
        public TelemetryRecord Validate(
            TelemetryPacket<double> packet)
        {
            var record =
                new TelemetryRecord
                {
                    SensorId =
                        packet.SensorId,

                    MetricName =
                        packet.MetricName,

                    Value =
                        packet.Value,

                    Unit =
                        packet.Unit,

                    Timestamp =
                        packet.Timestamp
                };

            EvaluateThreshold(record);

            return record;
        }

        private void EvaluateThreshold(
            TelemetryRecord record)
        {
            switch (
                record.MetricName.ToLower())
            {
                case "temperature":

                    if (record.Value < 0 ||
                        record.Value > 40)
                    {
                        record.IsAnomaly = true;

                        record.AnomalyReason =
                            "Temperature is outside the expected range of 0°C to 40°C.";
                    }

                    break;

                case "humidity":

                    if (record.Value < 20 ||
                        record.Value > 80)
                    {
                        record.IsAnomaly = true;

                        record.AnomalyReason =
                            "Humidity is outside the expected range of 20% to 80%.";
                    }

                    break;

                case "pressure":

                    if (record.Value < 950 ||
                        record.Value > 1050)
                    {
                        record.IsAnomaly = true;

                        record.AnomalyReason =
                            "Pressure is outside the expected range of 950 to 1050 hPa.";
                    }

                    break;

                case "power":

                    if (record.Value < 0 ||
                        record.Value > 5000)
                    {
                        record.IsAnomaly = true;

                        record.AnomalyReason =
                            "Power consumption is outside the expected operating range.";
                    }

                    break;

                default:

                    record.IsAnomaly = false;
                    record.AnomalyReason =
                        string.Empty;

                    break;
            }
        }
    }
}
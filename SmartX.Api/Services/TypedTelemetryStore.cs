using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class TypedTelemetryStore
    {
        private readonly List<TelemetryPacket<double>>
            _decimalTelemetry = new();

        private readonly List<TelemetryPacket<int>>
            _integerTelemetry = new();

        private readonly List<TelemetryPacket<bool>>
            _booleanTelemetry = new();


        public void Add(
            TelemetryPacket<double> packet)
        {
            _decimalTelemetry.Add(packet);
        }


        public void Add(
            TelemetryPacket<int> packet)
        {
            _integerTelemetry.Add(packet);
        }


        public void Add(
            TelemetryPacket<bool> packet)
        {
            _booleanTelemetry.Add(packet);
        }


        public IReadOnlyList<TelemetryPacket<double>>
            GetDecimalTelemetry()
        {
            return _decimalTelemetry;
        }


        public IReadOnlyList<TelemetryPacket<int>>
            GetIntegerTelemetry()
        {
            return _integerTelemetry;
        }


        public IReadOnlyList<TelemetryPacket<bool>>
            GetBooleanTelemetry()
        {
            return _booleanTelemetry;
        }
    }
}
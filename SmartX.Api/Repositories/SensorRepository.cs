using SmartX.Api.Models;

namespace SmartX.Api.Repositories
{
    public class SensorRepository : ISensorRepository
    {
        private readonly List<Sensor> _sensors = new();
        private int _nextId = 1;

        public Task<IEnumerable<Sensor>> GetAllAsync()
        {
            foreach (var sensor in _sensors)
            {
                sensor.Status =
                    sensor.CalculateStatus();
            }

            return Task.FromResult<IEnumerable<Sensor>>(
                _sensors);
        }

        public Task<Sensor?> GetByIdAsync(int id)
        {
            var sensor =
                _sensors.FirstOrDefault(
                    s => s.Id == id);

            if (sensor != null)
            {
                sensor.Status =
                    sensor.CalculateStatus();
            }

            return Task.FromResult(sensor);
        }

        public Task<Sensor?> GetByMacAddressAsync(string macAddress)
        {
            var sensor = _sensors.FirstOrDefault(
                s => s.MacAddress.Equals(
                    macAddress,
                    StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(sensor);
        }

        public Task<Sensor> AddAsync(Sensor sensor)
        {
            sensor.Id = _nextId++;
            sensor.DateRegistered = DateTime.UtcNow;
            sensor.LastSeen = DateTime.UtcNow;
            sensor.Status = SensorStatus.Connected;

            _sensors.Add(sensor);

            return Task.FromResult(sensor);
        }

        public Task<bool> DeleteAsync(int id)
        {
            var sensor = _sensors.FirstOrDefault(s => s.Id == id);

            if (sensor == null)
            {
                return Task.FromResult(false);
            }

            _sensors.Remove(sensor);

            return Task.FromResult(true);
        }
    }
}

using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Services
{
    public class DeviceLookupService
    {
        private readonly ISensorRepository _sensorRepository;

        private readonly Dictionary<string, Sensor> _deviceCache =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly object _sync = new();

        public DeviceLookupService(
            ISensorRepository sensorRepository)
        {
            _sensorRepository = sensorRepository;
        }

        public async Task<Sensor?> FindByMacAsync(string macAddress)
        {
            if (string.IsNullOrWhiteSpace(macAddress))
                return null;

            string key = NormaliseMac(macAddress);

            lock (_sync)
            {
                if (_deviceCache.TryGetValue(key, out var cached))
                    return cached;
            }

            var sensor = await _sensorRepository
                .GetByMacAddressAsync(macAddress.Trim());

            if (sensor == null)
            {
                var allSensors = await _sensorRepository.GetAllAsync();

                sensor = allSensors.FirstOrDefault(s =>
                    NormaliseMac(s.MacAddress) == key);
            }

            if (sensor != null)
            {
                lock (_sync)
                {
                    _deviceCache[key] = sensor;
                }
            }

            return sensor;
        }

        public async Task<int> RefreshCacheAsync()
        {
            var sensors = await _sensorRepository.GetAllAsync();

            lock (_sync)
            {
                _deviceCache.Clear();

                foreach (var sensor in sensors)
                {
                    _deviceCache[NormaliseMac(sensor.MacAddress)] =
                        sensor;
                }

                return _deviceCache.Count;
            }
        }

        public int CachedDeviceCount
        {
            get
            {
                lock (_sync)
                {
                    return _deviceCache.Count;
                }
            }
        }

        private static string NormaliseMac(string mac)
        {
            return mac.Trim()
                .Replace("-", "")
                .Replace(":", "")
                .ToUpperInvariant();
        }
    }
}

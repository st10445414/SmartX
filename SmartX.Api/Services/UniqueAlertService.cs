
namespace SmartX.Api.Services
{
    public class UniqueAlertService
    {
        private readonly HashSet<string> _activeAlerts =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly object _sync = new();

        public bool RegisterAlert(int sensorId, string alertType)
        {
            string key = CreateKey(sensorId, alertType);

            lock (_sync)
            {
                return _activeAlerts.Add(key);
            }
        }

        public bool ResolveAlert(int sensorId, string alertType)
        {
            string key = CreateKey(sensorId, alertType);

            lock (_sync)
            {
                return _activeAlerts.Remove(key);
            }
        }

        public bool HasActiveAlert(int sensorId, string alertType)
        {
            string key = CreateKey(sensorId, alertType);

            lock (_sync)
            {
                return _activeAlerts.Contains(key);
            }
        }

        public string[] GetActiveAlerts()
        {
            lock (_sync)
            {
                return _activeAlerts.OrderBy(x => x).ToArray();
            }
        }

        private static string CreateKey(int sensorId, string alertType)
        {
            return $"{sensorId}:{alertType.Trim().ToUpperInvariant()}";
        }
    }
}

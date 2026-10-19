using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public interface IActiveAlertApiService
    {
        Task<ActiveAlertsResponse?> GetAlertsAsync();
    }
}
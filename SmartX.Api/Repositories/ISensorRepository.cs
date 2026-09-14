using SmartX.Api.Models;

namespace SmartX.Api.Repositories
{
    public interface ISensorRepository
    {
        Task<IEnumerable<Sensor>> GetAllAsync();
        Task<Sensor?> GetByIdAsync(int id);
        Task<Sensor?> GetByMacAddressAsync(string macAddress);
        Task<Sensor> AddAsync(Sensor sensor);
        Task<bool> DeleteAsync(int id);
    }
}
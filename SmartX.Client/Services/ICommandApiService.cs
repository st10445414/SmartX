
using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public interface ICommandApiService
    {
        Task<CommandOperationResult> EnqueueAsync(
            DeviceCommandInput model);

        Task<CommandOperationResult> ProcessNextAsync();

        Task<CommandOperationResult> UndoAsync();

        Task<CommandStatusViewModel?> GetStatusAsync();
    }
}

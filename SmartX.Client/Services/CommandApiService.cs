
using System.Net.Http.Json;
using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public class CommandApiService : ICommandApiService
    {
        private readonly HttpClient _httpClient;

        public CommandApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CommandOperationResult> EnqueueAsync(
            DeviceCommandInput model)
        {
            return await PostAsync(
                "api/commands/enqueue", model,
                "Command successfully queued.");
        }

        public async Task<CommandOperationResult> ProcessNextAsync()
        {
            return await PostAsync(
                "api/commands/process-next", null,
                "Next queued command processed.");
        }

        public async Task<CommandOperationResult> UndoAsync()
        {
            return await PostAsync(
                "api/commands/undo", null,
                "Last command successfully undone.");
        }

        public async Task<CommandStatusViewModel?> GetStatusAsync()
        {
            try
            {
                return await _httpClient
                    .GetFromJsonAsync<CommandStatusViewModel>(
                        "api/commands/status");
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        private async Task<CommandOperationResult> PostAsync(
            string url, object? body, string successMessage)
        {
            try
            {
                using var response = body == null
                    ? await _httpClient.PostAsync(url, null)
                    : await _httpClient.PostAsJsonAsync(url, body);

                if (!response.IsSuccessStatusCode)
                {
                    return new CommandOperationResult
                    {
                        Success = false,
                        Message = $"API returned HTTP {(int)response.StatusCode}."
                    };
                }

                return new CommandOperationResult
                {
                    Success = true,
                    Message = successMessage
                };
            }
            catch (HttpRequestException)
            {
                return new CommandOperationResult
                {
                    Success = false,
                    Message = "Unable to connect to the Smart-X API."
                };
            }
            catch (TaskCanceledException)
            {
                return new CommandOperationResult
                {
                    Success = false,
                    Message = "The API request timed out."
                };
            }
        }
    }
}

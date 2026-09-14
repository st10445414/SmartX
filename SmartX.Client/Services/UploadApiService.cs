using System.Net.Http.Json;
using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public class UploadApiService :
        IUploadApiService
    {
        private readonly HttpClient
            _httpClient;

        public UploadApiService(
            HttpClient httpClient)
        {
            _httpClient =
                httpClient;
        }

        public async Task<
            ApiResult<SensorAttachmentViewModel>>
            UploadAsync(
                FileUploadViewModel model)
        {
            try
            {
                using var content =
                    new MultipartFormDataContent();

                content.Add(
                    new StringContent(
                        model.SensorId.ToString()),
                    "SensorId");

                content.Add(
                    new StringContent(
                        model.Description ?? ""),
                    "Description");

                using var fileStream =
                    model.File.OpenReadStream();

                using var fileContent =
                    new StreamContent(
                        fileStream);

                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers
                        .MediaTypeHeaderValue(
                            model.File.ContentType);

                content.Add(
                    fileContent,
                    "File",
                    model.File.FileName);

                var response =
                    await _httpClient.PostAsync(
                        "api/uploads",
                        content);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResult<
                        SensorAttachmentViewModel>
                    {
                        Success = false,
                        Message =
                            "The file could not be uploaded. Check its type and size."
                    };
                }

                var attachment =
                    await response.Content
                        .ReadFromJsonAsync<
                            SensorAttachmentViewModel>();

                return new ApiResult<
                    SensorAttachmentViewModel>
                {
                    Success = true,
                    Data = attachment,
                    Message =
                        "File uploaded successfully."
                };
            }
            catch (HttpRequestException)
            {
                return new ApiResult<
                    SensorAttachmentViewModel>
                {
                    Success = false,
                    Message =
                        "The Smart-X API could not be reached."
                };
            }
        }

        public async Task<
            List<SensorAttachmentViewModel>>
            GetSensorFilesAsync(
                int sensorId)
        {
            try
            {
                var files =
                    await _httpClient
                        .GetFromJsonAsync<
                            List<
                                SensorAttachmentViewModel>>(
                            $"api/uploads/sensor/{sensorId}");

                return files ??
                    new List<
                        SensorAttachmentViewModel>();
            }
            catch
            {
                return new List<
                    SensorAttachmentViewModel>();
            }
        }
    }
}
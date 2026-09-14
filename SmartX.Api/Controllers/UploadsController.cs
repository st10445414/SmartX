using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadsController :
        ControllerBase
    {
        private readonly ISensorRepository
            _sensorRepository;

        private readonly IAttachmentRepository
            _attachmentRepository;

        private readonly IWebHostEnvironment
            _environment;

        private static readonly string[]
            AllowedExtensions =
            {
                ".txt",
                ".log",
                ".csv",
                ".json",
                ".jpg",
                ".jpeg",
                ".png"
            };

        private const long MaxFileSize =
            5 * 1024 * 1024;

        public UploadsController(
            ISensorRepository sensorRepository,
            IAttachmentRepository attachmentRepository,
            IWebHostEnvironment environment)
        {
            _sensorRepository =
                sensorRepository;

            _attachmentRepository =
                attachmentRepository;

            _environment =
                environment;
        }

        [HttpPost]
        [RequestSizeLimit(MaxFileSize)]
        public async Task<ActionResult<SensorAttachment>>
            UploadFile(
                [FromForm] FileUploadRequest request)
        {
            var sensor =
                await _sensorRepository
                    .GetByIdAsync(
                        request.SensorId);

            if (sensor == null)
            {
                return NotFound(new
                {
                    message =
                        "The selected sensor could not be found."
                });
            }

            if (request.File == null ||
                request.File.Length == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Please select a file to upload."
                });
            }

            if (request.File.Length >
                MaxFileSize)
            {
                return BadRequest(new
                {
                    message =
                        "The selected file exceeds the 5 MB limit."
                });
            }

            var extension =
                Path.GetExtension(
                    request.File.FileName)
                .ToLowerInvariant();

            if (!AllowedExtensions
                .Contains(extension))
            {
                return BadRequest(new
                {
                    message =
                        "Unsupported file type."
                });
            }

            var uploadFolder =
                Path.Combine(
                    _environment.ContentRootPath,
                    "Uploads",
                    request.SensorId.ToString());

            Directory.CreateDirectory(
                uploadFolder);

            var uniqueFileName =
                $"{Guid.NewGuid()}{extension}";

            var filePath =
                Path.Combine(
                    uploadFolder,
                    uniqueFileName);

            await using (
                var fileStream =
                    new FileStream(
                        filePath,
                        FileMode.Create))
            {
                await request.File
                    .CopyToAsync(
                        fileStream);
            }

            var attachment =
                new SensorAttachment
                {
                    SensorId =
                        request.SensorId,

                    OriginalFileName =
                        Path.GetFileName(
                            request.File.FileName),

                    StoredFileName =
                        uniqueFileName,

                    ContentType =
                        request.File.ContentType,

                    FileSize =
                        request.File.Length,

                    UploadedAt =
                        DateTime.UtcNow,

                    Description =
                        request.Description
                };

            await _attachmentRepository
                .AddAsync(attachment);

            return Ok(attachment);
        }

        [HttpGet("sensor/{sensorId:int}")]
        public async Task<
            ActionResult<List<SensorAttachment>>>
            GetSensorFiles(
                int sensorId)
        {
            var files =
                await _attachmentRepository
                    .GetBySensorIdAsync(
                        sensorId);

            return Ok(files);
        }
    }
}
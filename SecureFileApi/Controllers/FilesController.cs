
using Microsoft.AspNetCore.Mvc;
using SecureFileApi.Services;

namespace SecureFileApi.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly IFileProcessingService _service;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileProcessingService service,
        ILogger<FilesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new
            {
                error = "A non-empty CSV file is required."
            });
        }

        if (file.Length > MaxFileSize)
        {
            return StatusCode(413, new
            {
                error = "Maximum file size is 5 MB."
            });
        }

        var fileName = Path.GetFileName(file.FileName);

        if (!string.Equals(
            Path.GetExtension(fileName),
            ".csv",
            StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                error = "Only CSV files are supported."
            });
        }

        _logger.LogInformation(
            "Received upload {FileName}, Size={Size}",
            fileName,
            file.Length);

        try
        {
            await using var stream = file.OpenReadStream();

            var result = _service.ProcessCsv(fileName, stream);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Upload failed for {FileName}", fileName);

            return Problem(
                statusCode: 500,
                title: "Unable to process the uploaded file.");
        }
    }

    [HttpGet("history")]
    public IActionResult GetHistory()
    {
        var records = _service.GetHistory();

        return Ok(new
        {
            totalFilesProcessed = _service.GetProcessedFileCount(),
            files = records
        });
    }

    [HttpGet("history/{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var record = _service.GetById(id);

        if (record is null)
        {
            return NotFound(new
            {
                error = "Processing record not found."
            });
        }

        return Ok(record);
    }
}

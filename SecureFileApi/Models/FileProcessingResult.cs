
namespace SecureFileApi.Models;

public class FileProcessingResult
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FileName { get; set; } = string.Empty;

    public int TotalRows { get; set; }

    public int SuccessfulRows { get; set; }

    public int FailedRows { get; set; }

    public decimal? AverageSalary { get; set; }

    public string Status { get; set; } = "Pending";

    public List<string> Errors { get; set; } = new();

    public DateTimeOffset ProcessedAt { get; set; }
}

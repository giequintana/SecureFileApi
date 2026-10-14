
namespace SecureFileApi.Models;

public class FileProcessingRecord
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public int TotalRows { get; set; }

    public int SuccessfulRows { get; set; }

    public int FailedRows { get; set; }

    public decimal? AverageSalary { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset ProcessedAt { get; set; }
}


using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using SecureFileApi.Models;

namespace SecureFileApi.Services;

public class FileProcessingService : IFileProcessingService
{
    private readonly ConcurrentDictionary<Guid, FileProcessingRecord>
        _history = new();

    private readonly ILogger<FileProcessingService> _logger;

    public FileProcessingService(
        ILogger<FileProcessingService> logger)
    {
        _logger = logger;
    }

    public FileProcessingResult ProcessCsv(
        string fileName,
        Stream stream)
    {
        var result = new FileProcessingResult
        {
            FileName = fileName,
            ProcessedAt = DateTimeOffset.UtcNow
        };

        try
        {
            using var parser = new TextFieldParser(stream)
            {
                TextFieldType = FieldType.Delimited,
                HasFieldsEnclosedInQuotes = true,
                TrimWhiteSpace = true
            };

            parser.SetDelimiters(",");

            if (parser.EndOfData)
                throw new InvalidDataException("CSV file is empty.");

            var headers = parser.ReadFields();

            var expectedHeaders =
                new[] { "Id", "Name", "Department", "Salary" };

            if (headers is null ||
                !headers.SequenceEqual(
                    expectedHeaders,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "Required columns: Id,Name,Department,Salary.");
            }

            decimal salaryTotal = 0;

            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                result.TotalRows++;

                if (fields is null || fields.Length != 4 ||
                    !int.TryParse(fields[0], out _) ||
                    string.IsNullOrWhiteSpace(fields[1]) ||
                    string.IsNullOrWhiteSpace(fields[2]) ||
                    !decimal.TryParse(
                        fields[3],
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var salary) ||
                    salary < 0)
                {
                    result.FailedRows++;
                    result.Errors.Add(
                        $"Row {result.TotalRows + 1}: Invalid data.");
                    continue;
                }

                result.SuccessfulRows++;
                salaryTotal += salary;
            }

            if (result.SuccessfulRows > 0)
            {
                result.AverageSalary =
                    salaryTotal / result.SuccessfulRows;
            }

            result.Status = result.FailedRows == 0
                ? "Completed"
                : result.SuccessfulRows == 0
                    ? "Failed"
                    : "CompletedWithErrors";
        }
        catch (Exception ex) when (
            ex is InvalidDataException ||
            ex is MalformedLineException ||
            ex is DecoderFallbackException)
        {
            _logger.LogWarning(
                ex, "Invalid CSV file {FileName}", fileName);

            result.Status = "Failed";
            result.FailedRows++;
            result.Errors.Add(
                "The CSV file is invalid or unreadable.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Unexpected error processing {FileName}", fileName);

            result.Status = "Failed";
            result.FailedRows++;
            result.Errors.Add("An unexpected processing error occurred.");
        }

        // Every processing attempt is tracked, including failures.
        var record = new FileProcessingRecord
        {
            Id = result.Id,
            FileName = result.FileName,
            TotalRows = result.TotalRows,
            SuccessfulRows = result.SuccessfulRows,
            FailedRows = result.FailedRows,
            AverageSalary = result.AverageSalary,
            Status = result.Status,
            ProcessedAt = result.ProcessedAt
        };

        _history[result.Id] = record;

        _logger.LogInformation(
            "Processed {FileName}: Status={Status}, Rows={Rows}, Id={Id}",
            result.FileName,
            result.Status,
            result.TotalRows,
            result.Id);

        return result;
    }

    public IEnumerable<FileProcessingRecord> GetHistory()
    {
        return _history.Values
            .OrderByDescending(x => x.ProcessedAt);
    }

    public FileProcessingRecord? GetById(Guid id)
    {
        return _history.TryGetValue(id, out var record)
            ? record
            : null;
    }

    public int GetProcessedFileCount()
    {
        return _history.Count;
    }
}

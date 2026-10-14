using SecureFileApi.Models;

namespace SecureFileApi.Services
{
    public interface IFileProcessingService
    {
        FileProcessingResult ProcessCsv(
        string fileName,
        Stream stream);

        IEnumerable<FileProcessingRecord> GetHistory();

        FileProcessingRecord? GetById(Guid id);

        int GetProcessedFileCount();
    }
}

namespace ReportGenerator.Application.Interfaces;

public interface IBlobStorageService
{
    /// <summary>
    /// Uploads the given stream as a blob and returns the downloadable URL (SAS URL).
    /// </summary>
    Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken = default);
}

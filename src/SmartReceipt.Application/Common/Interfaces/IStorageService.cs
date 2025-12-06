namespace SmartReceipt.Application.Common.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
    
    Task<Stream> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default);
    
    Task<string> GetFileUrlAsync(string fileUrl, CancellationToken cancellationToken = default);
}


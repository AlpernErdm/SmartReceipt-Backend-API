using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartReceipt.Application.Common.Interfaces;

namespace SmartReceipt.Infrastructure.Services;

public class StorageService : IStorageService
{
    private readonly ILogger<StorageService> _logger;
    private readonly string _storagePath;

    public StorageService(ILogger<StorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _storagePath = configuration["Storage:LocalPath"] ?? "wwwroot/uploads";
        
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_storagePath, uniqueFileName);

        using (var fileStream2 = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStream2, cancellationToken);
        }

        var fileUrl = $"/uploads/{uniqueFileName}";
        _logger.LogInformation("File uploaded: {FileName} -> {Url}", fileName, fileUrl);
        
        return fileUrl;
    }

    public async Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_storagePath, fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {FileName}", fileName);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FileUrl}", fileUrl);
            return false;
        }
    }

    public async Task<Stream> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(fileUrl);
        var filePath = Path.Combine(_storagePath, fileName);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {fileUrl}");
        }
        
        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public async Task<string> GetFileUrlAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return fileUrl;
    }
}


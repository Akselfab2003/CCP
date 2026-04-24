using MessagingService.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MessagingService.Infrastructure.Storage
{
    public class FileSystemAttachmentStorageService : IAttachmentStorageService
    {
        private readonly string _storagePath;

        public FileSystemAttachmentStorageService(IConfiguration configuration)
        {
            _storagePath = configuration["Attachments:StoragePath"] ?? "./attachments";
            Directory.CreateDirectory(_storagePath);
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string contentType, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(originalFileName);
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_storagePath, storedFileName);

            await using var fs = File.Create(filePath);
            await fileStream.CopyToAsync(fs, cancellationToken);

            return storedFileName;
        }

        public string? GetFilePath(string storedFileName)
        {
            var filePath = Path.Combine(_storagePath, Path.GetFileName(storedFileName));
            return File.Exists(filePath) ? filePath : null;
        }
    }
}

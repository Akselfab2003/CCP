namespace MessagingService.Application.Interfaces
{
    public interface IAttachmentStorageService
    {
        // Saves a file and returns its publicly accessible URL/path.
        Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string contentType, CancellationToken cancellationToken = default);

        // Returns the absolute filesystem path for a given stored filename, or null if not found.
        string? GetFilePath(string storedFileName);
    }
}

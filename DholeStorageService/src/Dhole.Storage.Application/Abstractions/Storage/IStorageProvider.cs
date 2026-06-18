using Dhole.Storage.Application.Abstractions.Files;

namespace Dhole.Storage.Application.Abstractions.Storage;

public interface IStorageProvider
{
    Task<string> UploadAsync(
        string fileName,
        string contentType,
        Stream content,
        string? folder,
        CancellationToken cancellationToken = default
    );

    Task<FileContent?> DownloadAsync(
        string storageKey,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}

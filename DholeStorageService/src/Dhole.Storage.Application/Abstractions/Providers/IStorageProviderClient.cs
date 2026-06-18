using Dhole.Storage.Application.Abstractions.Files;

namespace Dhole.Storage.Application.Abstractions.Providers;

public interface IStorageProviderClient
{
    string ProviderType { get; }

    Task<StoredFileResult> SaveAsync(
        FileContent file,
        string storagePath,
        CancellationToken cancellationToken = default
    );

    Task<DownloadFileResult> DownloadAsync(
        string storagePath,
        string fileName,
        string contentType,
        long sizeInBytes,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);

    Task<string> CreateSignedDownloadUrlAsync(
        string storagePath,
        string fileName,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    );
}

using Dhole.Storage.Contracts.Files;

namespace Dhole.Storage.Application.Abstractions.Cache;

public interface IStorageCacheService
{
    Task<DownloadFileResponse?> GetSignedDownloadUrlAsync(
        Guid fileId,
        CancellationToken cancellationToken = default
    );

    Task SetSignedDownloadUrlAsync(
        Guid fileId,
        DownloadFileResponse response,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    );

    Task RemoveSignedDownloadUrlAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StorageFileDto>?> GetFilesByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default
    );

    Task SetFilesByEntityAsync(
        string entityType,
        Guid entityId,
        IReadOnlyCollection<StorageFileDto> files,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    );

    Task RemoveFilesByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default
    );
}

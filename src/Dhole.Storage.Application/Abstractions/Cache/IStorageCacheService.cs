using Dhole.Storage.Contracts.Files.Response;

namespace Dhole.Storage.Application.Abstractions.Cache;

public interface IStorageCacheService
{
    Task<DownloadFileDto?> GetSignedDownloadUrlAsync(
        Guid fileId,
        CancellationToken cancellationToken = default
    );

    Task SetSignedDownloadUrlAsync(
        Guid fileId,
        DownloadFileDto response,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    );

    Task RemoveSignedDownloadUrlAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FileDto>?> GetFilesByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default
    );

    Task SetFilesByEntityAsync(
        string entityType,
        Guid entitId,
        IReadOnlyCollection<FileDto> files,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    );

    Task RemoveFilesByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default
    );
}

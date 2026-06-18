using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Caching;
using Dhole.Storage.Application.Abstractions.Cache;
using Dhole.Storage.Contracts.Files;

namespace Dhole.Storage.Infrastructure.Cache;

public sealed class StorageCacheService(ICacheService cache) : IStorageCacheService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(15);

    public Task<DownloadFileResponse?> GetSignedDownloadUrlAsync(
        Guid fileId,
        CancellationToken cancellationToken = default
    )
    {
        return cache.GetAsync<DownloadFileResponse>(
            StorageCacheKeys.SignedDownloadUrl(fileId),
            cancellationToken
        );
    }

    public Task SetSignedDownloadUrlAsync(
        Guid fileId,
        DownloadFileResponse response,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    )
    {
        return cache.SetAsync(
            StorageCacheKeys.SignedDownloadUrl(fileId),
            response,
            CacheEntryOptions.Default(expiration),
            cancellationToken
        );
    }

    public Task RemoveSignedDownloadUrlAsync(
        Guid fileId,
        CancellationToken cancellationToken = default
    )
    {
        return cache.RemoveAsync(StorageCacheKeys.SignedDownloadUrl(fileId), cancellationToken);
    }

    public Task<IReadOnlyCollection<StorageFileDto>?> GetFilesByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default
    )
    {
        return cache.GetAsync<IReadOnlyCollection<StorageFileDto>>(
            StorageCacheKeys.FilesByEntity(entityType, entityId),
            cancellationToken
        );
    }

    public Task SetFilesByEntityAsync(
        string entityType,
        Guid entityId,
        IReadOnlyCollection<StorageFileDto> files,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
    {
        return cache.SetAsync(
            StorageCacheKeys.FilesByEntity(entityType, entityId),
            files,
            CacheEntryOptions.Default(expiration ?? DefaultExpiration),
            cancellationToken
        );
    }

    public Task RemoveFilesByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default
    )
    {
        return cache.RemoveAsync(
            StorageCacheKeys.FilesByEntity(entityType, entityId),
            cancellationToken
        );
    }
}

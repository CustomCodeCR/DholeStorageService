using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Storage.Contracts.Files;
using Dhole.Storage.Domain.Files.Entities;

namespace Dhole.Storage.Application.Abstractions.Repositories;

public interface IStorageFileRepository : IRepository<StorageFile, Guid>
{
    Task<StorageFile?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StorageFileDto>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<StorageFileVersionDto>> GetVersionsAsync(
        Guid fileId,
        CancellationToken cancellationToken = default
    );

    Task<PagedResult<StorageFileDto>> GetPagedAsync(
        PageRequest page,
        string? sourceService = null,
        string? entityType = null,
        Guid? entityId = null,
        string? documentType = null,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<StorageFileSelectDto>> GetForSelectAsync(
        string? sourceService = null,
        string? entityType = null,
        Guid? entityId = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );
}

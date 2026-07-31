using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Storage.Contracts.Files.Response;

namespace Dhole.Storage.Application.Abstractions.Repositories;

public interface IFileRepository : IRepository<Dhole.Storage.Domain.Files.Entities.File, Guid>
{
    Task<Dhole.Storage.Domain.Files.Entities.File?> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<FileDto>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<FileVersionDto>> GetVersionsAsync(
        Guid fileId,
        CancellationToken cancellationToken = default
    );

    Task<PagedResult<FileDto>> GetPagedAsync(
        PageRequest page,
        string? sourceService = null,
        string? entityType = null,
        Guid? entityId = null,
        string? documentType = null,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<FileSelectDto>> GetForSelectAsync(
        string? sourceService = null,
        string? entityType = null,
        Guid? entityId = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );
}

using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Storage.Contracts.Providers;
using Dhole.Storage.Domain.Providers.Entities;

namespace Dhole.Storage.Application.Abstractions.Repositories;

public interface IStorageProviderRepository : IRepository<StorageProvider, Guid>
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<StorageProvider?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    );

    Task<StorageProvider?> GetDefaultAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<StorageProviderDto>> GetPagedAsync(
        PageRequest page,
        string? search = null,
        string? providerType = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<StorageProviderSelectDto>> GetForSelectAsync(
        string? providerType = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );
}

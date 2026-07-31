using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Storage.Contracts.Providers.Response;
using Dhole.Storage.Domain.Providers.Entities;

namespace Dhole.Storage.Application.Abstractions.Repositories;

public interface IProviderRepository : IRepository<Provider, Guid>
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<Provider?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<Provider?> GetDefaultAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<ProviderDto>> GetPagedAsync(
        PageRequest page,
        string? search = null,
        string? providerType = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ProviderSelectDto>> GetForSelectAsync(
        string? providerType = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );
}

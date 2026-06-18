using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Contracts.Providers;

namespace Dhole.Storage.Application.Providers.GetStorageProviders;

public sealed class GetStorageProvidersQueryHandler(IStorageProviderRepository providers)
    : IQueryHandler<GetStorageProvidersQuery, PagedResult<StorageProviderDto>>
{
    public Task<PagedResult<StorageProviderDto>> HandleAsync(
        GetStorageProvidersQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return providers.GetPagedAsync(
            query.Page,
            query.Search,
            query.ProviderType,
            query.IsActive,
            cancellationToken
        );
    }
}

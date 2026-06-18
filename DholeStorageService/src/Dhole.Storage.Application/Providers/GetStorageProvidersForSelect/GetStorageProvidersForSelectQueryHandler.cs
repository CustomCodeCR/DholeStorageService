using CustomCodeFramework.Cqrs.Queries;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Contracts.Providers;

namespace Dhole.Storage.Application.Providers.GetStorageProvidersForSelect;

public sealed class GetStorageProvidersForSelectQueryHandler(IStorageProviderRepository providers)
    : IQueryHandler<GetStorageProvidersForSelectQuery, IReadOnlyCollection<StorageProviderSelectDto>>
{
    public Task<IReadOnlyCollection<StorageProviderSelectDto>> HandleAsync(
        GetStorageProvidersForSelectQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return providers.GetForSelectAsync(query.ProviderType, query.Search, cancellationToken);
    }
}

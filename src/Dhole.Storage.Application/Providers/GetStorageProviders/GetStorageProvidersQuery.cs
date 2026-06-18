using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Storage.Contracts.Providers;

namespace Dhole.Storage.Application.Providers.GetStorageProviders;

public sealed record GetStorageProvidersQuery(
    PageRequest Page,
    string? Search,
    string? ProviderType,
    bool? IsActive
) : IQuery<PagedResult<StorageProviderDto>>;

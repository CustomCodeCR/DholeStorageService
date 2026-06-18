using CustomCodeFramework.Cqrs.Queries;
using Dhole.Storage.Contracts.Providers;

namespace Dhole.Storage.Application.Providers.GetStorageProvidersForSelect;

public sealed record GetStorageProvidersForSelectQuery(string? ProviderType, string? Search)
    : IQuery<IReadOnlyCollection<StorageProviderSelectDto>>;

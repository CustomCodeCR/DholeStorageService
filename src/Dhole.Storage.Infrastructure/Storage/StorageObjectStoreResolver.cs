using Dhole.Storage.Application.Abstractions.Storage;
using Dhole.Storage.Domain.Providers.Enums;

namespace Dhole.Storage.Infrastructure.Storage;

public sealed class StorageObjectStoreResolver(IEnumerable<IStorageObjectStore> stores)
    : IStorageObjectStoreResolver
{
    private readonly IReadOnlyDictionary<ProviderType, IStorageObjectStore> _stores = stores
        .GroupBy(x => x.ProviderType)
        .ToDictionary(x => x.Key, x => x.First());

    public IStorageObjectStore Resolve(ProviderType providerType)
    {
        return _stores.TryGetValue(providerType, out var store)
            ? store
            : throw new InvalidOperationException(
                $"No se registró un almacén físico para el proveedor '{providerType}'."
            );
    }
}

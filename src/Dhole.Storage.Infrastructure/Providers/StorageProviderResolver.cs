using Dhole.Storage.Application.Abstractions.Storage;
using Dhole.Storage.Domain.Providers.Enums;

namespace Dhole.Storage.Infrastructure.Providers;

/// <summary>
/// Compatibilidad con instalaciones anteriores que todavía conservan este archivo.
/// La resolución real se realiza mediante IStorageObjectStoreResolver.
/// </summary>
[Obsolete("Use IStorageObjectStoreResolver directamente.")]
public sealed class StorageProviderResolver(IStorageObjectStoreResolver resolver)
{
    public IStorageObjectStore Resolve(ProviderType providerType)
    {
        return resolver.Resolve(providerType);
    }
}

using Dhole.Storage.Domain.Providers.Enums;

namespace Dhole.Storage.Application.Abstractions.Storage;

public interface IStorageObjectStoreResolver
{
    IStorageObjectStore Resolve(ProviderType providerType);
}

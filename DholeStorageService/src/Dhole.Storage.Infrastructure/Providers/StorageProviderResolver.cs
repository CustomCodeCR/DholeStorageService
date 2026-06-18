using CustomCodeFramework.Storage.Abstractions;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Infrastructure.Providers;

public sealed class StorageProviderResolver(IEnumerable<IFileStorageProvider> providers)
{
    public IFileStorageProvider Resolve(string providerType)
    {
        var providerName = providerType switch
        {
            StorageConstants.ProviderTypes.Local => "Local",
            StorageConstants.ProviderTypes.S3 => "S3",
            StorageConstants.ProviderTypes.AzureBlob => "AzureBlob",
            StorageConstants.ProviderTypes.MinIO => "S3",
            _ => providerType,
        };

        return providers.First(x =>
            x.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase)
        );
    }
}

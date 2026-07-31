using Dhole.Storage.Application.Abstractions.Storage;
using Dhole.Storage.Domain.Providers.Enums;

namespace Dhole.Storage.Infrastructure.Storage;

public sealed class MinioStorageObjectStore(S3StorageObjectStore inner) : IStorageObjectStore
{
    public ProviderType ProviderType => ProviderType.MinIO;

    public Task WriteAsync(
        string path,
        Stream content,
        string contentType,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    ) => inner.WriteAsync(path, content, contentType, providerConfiguration, cancellationToken);

    public Task<StorageObjectReadResult> ReadAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    ) => inner.ReadAsync(path, providerConfiguration, cancellationToken);

    public Task DeleteAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    ) => inner.DeleteAsync(path, providerConfiguration, cancellationToken);

    public Task<bool> ExistsAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    ) => inner.ExistsAsync(path, providerConfiguration, cancellationToken);
}

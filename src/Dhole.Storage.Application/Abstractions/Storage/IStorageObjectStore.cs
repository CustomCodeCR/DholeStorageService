using Dhole.Storage.Domain.Providers.Enums;

namespace Dhole.Storage.Application.Abstractions.Storage;

public interface IStorageObjectStore
{
    ProviderType ProviderType { get; }

    Task WriteAsync(
        string path,
        Stream content,
        string contentType,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    );

    Task<StorageObjectReadResult> ReadAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    );
}

public sealed record StorageObjectReadResult(byte[] Content, string? ContentType = null);

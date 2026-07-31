using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dhole.Storage.Application.Abstractions.Storage;
using Dhole.Storage.Domain.Providers.Enums;

namespace Dhole.Storage.Infrastructure.Storage;

public sealed class AzureBlobStorageObjectStore : IStorageObjectStore
{
    public ProviderType ProviderType => ProviderType.AzureBlob;

    public async Task WriteAsync(
        string path,
        Stream content,
        string contentType,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    )
    {
        var (_, container) = await CreateContainerAsync(providerConfiguration, cancellationToken);
        var blob = container.GetBlobClient(NormalizePath(path));

        if (content.CanSeek)
        {
            content.Position = 0;
        }

        await blob.UploadAsync(content, overwrite: true, cancellationToken: cancellationToken);
        await blob.SetHttpHeadersAsync(
            new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken
        );

        if (content.CanSeek)
        {
            content.Position = 0;
        }
    }

    public async Task<StorageObjectReadResult> ReadAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    )
    {
        var (_, container) = await CreateContainerAsync(providerConfiguration, cancellationToken);
        var response = await container
            .GetBlobClient(NormalizePath(path))
            .DownloadContentAsync(cancellationToken);

        return new StorageObjectReadResult(
            response.Value.Content.ToArray(),
            response.Value.Details.ContentType
        );
    }

    public async Task DeleteAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    )
    {
        var (_, container) = await CreateContainerAsync(providerConfiguration, cancellationToken);
        await container
            .GetBlobClient(NormalizePath(path))
            .DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    )
    {
        var (_, container) = await CreateContainerAsync(providerConfiguration, cancellationToken);
        return (await container.GetBlobClient(NormalizePath(path)).ExistsAsync(cancellationToken)).Value;
    }

    private static async Task<(AzureBlobProviderOptions Options, BlobContainerClient Container)> CreateContainerAsync(
        string? providerConfiguration,
        CancellationToken cancellationToken
    )
    {
        var options = StorageProviderConfiguration.Parse<AzureBlobProviderOptions>(
            providerConfiguration
        );

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException(
                "La configuración de Azure Blob requiere ConnectionString."
            );
        }

        if (string.IsNullOrWhiteSpace(options.ContainerName))
        {
            throw new InvalidOperationException(
                "La configuración de Azure Blob requiere ContainerName."
            );
        }

        var container = new BlobContainerClient(
            options.ConnectionString.Trim(),
            options.ContainerName.Trim()
        );
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        return (options, container);
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/').TrimStart('/');
}

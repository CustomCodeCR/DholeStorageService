using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Dhole.Storage.Application.Abstractions.Storage;
using Dhole.Storage.Domain.Providers.Enums;

namespace Dhole.Storage.Infrastructure.Storage;

public sealed class S3StorageObjectStore : IStorageObjectStore
{
    public ProviderType ProviderType => ProviderType.S3;

    public async Task WriteAsync(
        string path,
        Stream content,
        string contentType,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    )
    {
        var options = GetOptions(providerConfiguration);
        using var client = CreateClient(options);

        if (content.CanSeek)
        {
            content.Position = 0;
        }

        var request = new PutObjectRequest
        {
            BucketName = options.BucketName,
            Key = NormalizePath(path),
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false,
        };

        await client.PutObjectAsync(request, cancellationToken);

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
        var options = GetOptions(providerConfiguration);
        using var client = CreateClient(options);
        using var response = await client.GetObjectAsync(
            new GetObjectRequest
            {
                BucketName = options.BucketName,
                Key = NormalizePath(path),
            },
            cancellationToken
        );

        using var memory = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memory, cancellationToken);
        return new StorageObjectReadResult(memory.ToArray(), response.Headers.ContentType);
    }

    public async Task DeleteAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    )
    {
        var options = GetOptions(providerConfiguration);
        using var client = CreateClient(options);
        await client.DeleteObjectAsync(
            new DeleteObjectRequest
            {
                BucketName = options.BucketName,
                Key = NormalizePath(path),
            },
            cancellationToken
        );
    }

    public async Task<bool> ExistsAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    )
    {
        var options = GetOptions(providerConfiguration);
        using var client = CreateClient(options);

        try
        {
            await client.GetObjectMetadataAsync(
                new GetObjectMetadataRequest
                {
                    BucketName = options.BucketName,
                    Key = NormalizePath(path),
                },
                cancellationToken
            );
            return true;
        }
        catch (AmazonS3Exception exception) when ((int)exception.StatusCode == 404)
        {
            return false;
        }
    }

    private static S3ProviderOptions GetOptions(string? providerConfiguration)
    {
        var options = StorageProviderConfiguration.Parse<S3ProviderOptions>(
            providerConfiguration
        );

        if (string.IsNullOrWhiteSpace(options.BucketName))
        {
            throw new InvalidOperationException(
                "La configuración del proveedor S3/MinIO requiere BucketName."
            );
        }

        return options;
    }

    private static AmazonS3Client CreateClient(S3ProviderOptions options)
    {
        var config = new AmazonS3Config
        {
            ForcePathStyle = options.ForcePathStyle,
            UseHttp = options.UseHttp,
        };

        if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
        {
            config.ServiceURL = options.ServiceUrl.Trim();
            config.AuthenticationRegion = string.IsNullOrWhiteSpace(options.Region)
                ? "us-east-1"
                : options.Region.Trim();
        }
        else if (!string.IsNullOrWhiteSpace(options.Region))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region.Trim());
        }

        if (!string.IsNullOrWhiteSpace(options.AccessKey))
        {
            if (string.IsNullOrWhiteSpace(options.SecretKey))
            {
                throw new InvalidOperationException(
                    "La configuración S3 contiene AccessKey, pero no SecretKey."
                );
            }

            return new AmazonS3Client(
                new BasicAWSCredentials(options.AccessKey.Trim(), options.SecretKey.Trim()),
                config
            );
        }

        return new AmazonS3Client(config);
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/').TrimStart('/');
}

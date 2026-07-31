using System.Text.Json;

namespace Dhole.Storage.Infrastructure.Storage;

internal static class StorageProviderConfiguration
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    public static T Parse<T>(string? configuration)
        where T : new()
    {
        if (string.IsNullOrWhiteSpace(configuration))
        {
            return new T();
        }

        try
        {
            return JsonSerializer.Deserialize<T>(configuration, JsonOptions) ?? new T();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "La configuración JSON del proveedor de almacenamiento no es válida.",
                exception
            );
        }
    }
}

internal sealed class LocalProviderOptions
{
    public string? RootPath { get; init; }
}

internal sealed class S3ProviderOptions
{
    public string? BucketName { get; init; }
    public string? ServiceUrl { get; init; }
    public string? Region { get; init; }
    public string? AccessKey { get; init; }
    public string? SecretKey { get; init; }
    public bool ForcePathStyle { get; init; } = true;
    public bool UseHttp { get; init; }
}

internal sealed class AzureBlobProviderOptions
{
    public string? ConnectionString { get; init; }
    public string? ContainerName { get; init; }
}

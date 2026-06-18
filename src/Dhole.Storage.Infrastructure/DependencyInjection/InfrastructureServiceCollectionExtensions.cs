using CustomCodeFramework.Auth.DependencyInjection;
using CustomCodeFramework.Mongo.DependencyInjection;
using CustomCodeFramework.Redis.DependencyInjection;
using CustomCodeFramework.Storage.AzureBlob.DependencyInjection;
using CustomCodeFramework.Storage.DependencyInjection;
using CustomCodeFramework.Storage.Local.DependencyInjection;
using CustomCodeFramework.Storage.S3.DependencyInjection;
using Dhole.Storage.Application.Abstractions.Cache;
using Dhole.Storage.Application.Abstractions.Mongo;
using Dhole.Storage.Infrastructure.Cache;
using Dhole.Storage.Infrastructure.Mongo;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Storage.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddCustomCodeAuth(configuration);

        services.PostConfigure<AuthenticationOptions>(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        services.AddCustomCodeRedis(configuration);
        services.AddCustomCodeMongo(configuration);

        services.AddCustomCodeStorage(configuration);
        services.AddConfiguredStorageProvider(configuration);

        services.AddScoped<IStorageCacheService, StorageCacheService>();
        services.AddScoped<IStorageFileMetadataSnapshotWriter, StorageFileMetadataSnapshotWriter>();

        return services;
    }

    private static IServiceCollection AddConfiguredStorageProvider(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var providerName = configuration.GetValue<string>("Storage:ProviderName") ?? "Local";

        return providerName.Trim().ToLowerInvariant() switch
        {
            "local" => services.AddCustomCodeLocalStorage(configuration),
            "s3" or "minio" => services.AddCustomCodeS3Storage(configuration),
            "azureblob" or "azure-blob" or "azure_blob" => services.AddCustomCodeAzureBlobStorage(
                configuration
            ),
            _ => throw new InvalidOperationException(
                $"Storage provider '{providerName}' is not supported."
            ),
        };
    }
}

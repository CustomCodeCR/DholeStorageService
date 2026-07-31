using CustomCodeFramework.Auth.DependencyInjection;
using CustomCodeFramework.Mongo.DependencyInjection;
using CustomCodeFramework.Redis.DependencyInjection;
using Dhole.Storage.Application.Abstractions.Cache;
using Dhole.Storage.Application.Abstractions.Mongo;
using Dhole.Storage.Application.Abstractions.Storage;
using Dhole.Storage.Infrastructure.Cache;
using Dhole.Storage.Infrastructure.Mongo;
using Dhole.Storage.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Storage.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool includeWebAuthentication = true
    )
    {
        if (includeWebAuthentication)
        {
            services.AddCustomCodeAuth(configuration);

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            });
        }

        services.AddCustomCodeRedis(configuration);
        services.AddCustomCodeMongo(configuration);

        services.AddSingleton<LocalStorageObjectStore>();
        services.AddSingleton<S3StorageObjectStore>();
        services.AddSingleton<AzureBlobStorageObjectStore>();
        services.AddSingleton<IStorageObjectStore>(sp =>
            sp.GetRequiredService<LocalStorageObjectStore>()
        );
        services.AddSingleton<IStorageObjectStore>(sp =>
            sp.GetRequiredService<S3StorageObjectStore>()
        );
        services.AddSingleton<IStorageObjectStore>(sp =>
            new MinioStorageObjectStore(sp.GetRequiredService<S3StorageObjectStore>())
        );
        services.AddSingleton<IStorageObjectStore>(sp =>
            sp.GetRequiredService<AzureBlobStorageObjectStore>()
        );
        services.AddSingleton<IStorageObjectStoreResolver, StorageObjectStoreResolver>();

        services.AddScoped<IStorageCacheService, StorageCacheService>();
        services.AddScoped<IFileMetadataSnapshotWriter, FileMetadataSnapshotWriter>();

        return services;
    }
}

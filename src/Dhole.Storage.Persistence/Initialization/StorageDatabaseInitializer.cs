using System.Text.Json;
using Dhole.Storage.Domain.Providers.Entities;
using Dhole.Storage.Domain.Providers.Enums;
using Dhole.Storage.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dhole.Storage.Persistence.Initialization;

public static class StorageDatabaseInitializer
{
    private const string EnvironmentMinioProviderCode = "STO-MINIO-ENV";

    public static async Task<StorageInitializationResult> InitializeAsync(
        ServiceDbContext dbContext,
        IConfiguration configuration,
        CancellationToken cancellationToken = default
    )
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        var minio = BuildMinioConfiguration(configuration);
        if (minio is not null)
        {
            return await SynchronizeMinioProviderAsync(
                dbContext,
                minio,
                cancellationToken
            );
        }

        if (await dbContext.Providers.AnyAsync(x => !x.IsDeleted, cancellationToken))
        {
            return new StorageInitializationResult(
                ProviderSynchronized: false,
                ProviderCode: null,
                ProviderType: null,
                IsDefault: false
            );
        }

        return await EnsureLocalFallbackAsync(dbContext, configuration, cancellationToken);
    }

    private static async Task<StorageInitializationResult> SynchronizeMinioProviderAsync(
        ServiceDbContext dbContext,
        MinioEnvironmentConfiguration minio,
        CancellationToken cancellationToken
    )
    {
        var providerConfiguration = JsonSerializer.Serialize(
            new
            {
                bucketName = minio.BucketName,
                serviceUrl = minio.Endpoint,
                region = "us-east-1",
                accessKeyReference = "MINIO_ROOT_USER",
                secretKeyReference = "MINIO_ROOT_PASSWORD",
                forcePathStyle = true,
                useHttp = !minio.UseSsl,
                createBucketIfMissing = true,
            }
        );

        var provider = await dbContext.Providers.FirstOrDefaultAsync(
            x => x.Code == EnvironmentMinioProviderCode && !x.IsDeleted,
            cancellationToken
        );

        if (provider is null)
        {
            provider = Provider.Create(
                EnvironmentMinioProviderCode,
                "MinIO del ambiente",
                ProviderType.MinIO,
                providerConfiguration,
                isDefault: true,
                createdBy: null
            );
            dbContext.Providers.Add(provider);
        }
        else
        {
            provider.Update(
                "MinIO del ambiente",
                providerConfiguration,
                isDefault: true,
                updatedBy: null
            );
            provider.SetActive(true, null);
        }

        var otherDefaults = await dbContext.Providers
            .Where(x => x.Id != provider.Id && x.IsDefault && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var current in otherDefaults)
        {
            current.Update(
                current.Name,
                current.Configuration,
                isDefault: false,
                updatedBy: null
            );
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // API y Worker pueden iniciar al mismo tiempo. Si el otro proceso ya
            // creó el proveedor del ambiente, se reutiliza el registro persistido.
            dbContext.ChangeTracker.Clear();
            provider = await dbContext.Providers.AsNoTracking().FirstOrDefaultAsync(
                x => x.Code == EnvironmentMinioProviderCode && !x.IsDeleted,
                cancellationToken
            );

            if (provider is null)
            {
                throw;
            }
        }

        return new StorageInitializationResult(
            ProviderSynchronized: true,
            ProviderCode: provider.Code,
            ProviderType: provider.ProviderType.ToString(),
            IsDefault: provider.IsDefault
        );
    }

    private static async Task<StorageInitializationResult> EnsureLocalFallbackAsync(
        ServiceDbContext dbContext,
        IConfiguration configuration,
        CancellationToken cancellationToken
    )
    {
        var configuredRootPath = configuration["Storage:Local:RootPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "storage", "objects");
        var rootPath = Path.GetFullPath(configuredRootPath);
        Directory.CreateDirectory(rootPath);
        var providerConfiguration = JsonSerializer.Serialize(new { rootPath });
        var provider = Provider.Create(
            "STO-LOCAL-DEFAULT",
            "Almacenamiento local predeterminado",
            ProviderType.Local,
            providerConfiguration,
            isDefault: true,
            createdBy: null
        );

        dbContext.Providers.Add(provider);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
            provider = await dbContext.Providers.AsNoTracking().FirstOrDefaultAsync(
                x => x.Code == "STO-LOCAL-DEFAULT" && !x.IsDeleted,
                cancellationToken
            );

            if (provider is null)
            {
                throw;
            }
        }

        return new StorageInitializationResult(
            ProviderSynchronized: true,
            ProviderCode: provider.Code,
            ProviderType: provider.ProviderType.ToString(),
            IsDefault: provider.IsDefault
        );
    }

    private static MinioEnvironmentConfiguration? BuildMinioConfiguration(
        IConfiguration configuration
    )
    {
        var endpoint = Read(configuration, "MINIO_ENDPOINT");
        var rootUser = Read(configuration, "MINIO_ROOT_USER");
        var rootPassword = Read(configuration, "MINIO_ROOT_PASSWORD");

        if (
            string.IsNullOrWhiteSpace(endpoint)
            || string.IsNullOrWhiteSpace(rootUser)
            || string.IsNullOrWhiteSpace(rootPassword)
        )
        {
            return null;
        }

        var bucketName = Read(configuration, "STORAGE_MINIO_BUCKET")
            ?? Read(configuration, "MINIO_BUCKET")
            ?? "dhole-storage";

        var endpointUsesSsl = Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri)
            && string.Equals(endpointUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        var useSsl = bool.TryParse(Read(configuration, "MINIO_USE_SSL"), out var configuredSsl)
            ? configuredSsl
            : endpointUsesSsl;

        return new MinioEnvironmentConfiguration(endpoint, bucketName, useSsl);
    }

    private static string? Read(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record MinioEnvironmentConfiguration(
        string Endpoint,
        string BucketName,
        bool UseSsl
    );

    public sealed record StorageInitializationResult(
        bool ProviderSynchronized,
        string? ProviderCode,
        string? ProviderType,
        bool IsDefault
    );
}

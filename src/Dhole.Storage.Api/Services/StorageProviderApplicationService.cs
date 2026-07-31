using Dhole.Storage.Application.Abstractions.Auditing;
using Dhole.Storage.Application.Abstractions.Services;
using Dhole.Storage.Contracts.Providers.Request;
using Dhole.Storage.Contracts.Providers.Response;
using Dhole.Storage.Domain.Providers.Entities;
using Dhole.Storage.Domain.Providers.Enums;
using Dhole.Storage.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Storage.Api.Services;

public sealed class StorageProviderApplicationService(
    ServiceDbContext dbContext,
    ICodeGenerator codeGenerator,
    IStorageAuditService auditService
)
{
    public async Task<IReadOnlyCollection<ProviderDto>> GetAllAsync(
        CancellationToken cancellationToken
    )
    {
        var providers = await dbContext
            .Providers.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return providers.Select(ToDto).ToArray();
    }

    public async Task<ProviderDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var provider = await dbContext
            .Providers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        return provider is null ? null : ToDto(provider);
    }

    public async Task<ProviderDto> CreateAsync(
        CreateProviderRequest request,
        Guid? actorUserId,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("El nombre del proveedor es requerido.");
        }

        if (!Enum.TryParse<ProviderType>(request.ProviderType, true, out var providerType))
        {
            throw new InvalidOperationException(
                "ProviderType debe ser Local, MinIO, S3 o AzureBlob."
            );
        }

        if (request.IsDefault)
        {
            await UnsetCurrentDefaultAsync(actorUserId, cancellationToken);
        }

        var code = await codeGenerator.GenerateProviderCodeAsync(
            (candidate, token) =>
                dbContext.Providers.AnyAsync(
                    x => x.Code == candidate && !x.IsDeleted,
                    token
                ),
            cancellationToken
        );
        var provider = Provider.Create(
            code,
            request.Name,
            providerType,
            request.Configuration,
            request.IsDefault,
            actorUserId
        );

        dbContext.Providers.Add(provider);
        await auditService.PublishAsync(
            new StorageAuditEvent(
                "storage.provider.created",
                "Create",
                "StorageProvider",
                provider.Id,
                actorUserId,
                After: ToDto(provider)
            ),
            cancellationToken
        );
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(provider);
    }

    public async Task<ProviderDto> UpdateAsync(
        Guid id,
        UpdateProviderRequest request,
        Guid? actorUserId,
        CancellationToken cancellationToken
    )
    {
        var provider = await GetTrackedAsync(id, cancellationToken);
        var before = ToDto(provider);

        if (request.IsDefault)
        {
            await UnsetCurrentDefaultAsync(actorUserId, cancellationToken, provider.Id);
        }

        provider.Update(
            string.IsNullOrWhiteSpace(request.Name) ? provider.Name : request.Name,
            request.Configuration,
            request.IsDefault,
            actorUserId
        );

        await auditService.PublishAsync(
            new StorageAuditEvent(
                "storage.provider.updated",
                "Update",
                "StorageProvider",
                provider.Id,
                actorUserId,
                Before: before,
                After: ToDto(provider)
            ),
            cancellationToken
        );
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(provider);
    }

    public async Task<ProviderDto> SetActiveAsync(
        Guid id,
        bool isActive,
        Guid? actorUserId,
        CancellationToken cancellationToken
    )
    {
        var provider = await GetTrackedAsync(id, cancellationToken);

        if (!isActive && provider.IsDefault)
        {
            throw new InvalidOperationException(
                "No puede inactivar el proveedor predeterminado. Asigne otro primero."
            );
        }

        var before = provider.IsActive;
        provider.SetActive(isActive, actorUserId);
        await auditService.PublishAsync(
            new StorageAuditEvent(
                isActive ? "storage.provider.activated" : "storage.provider.inactivated",
                isActive ? "Activate" : "Inactivate",
                "StorageProvider",
                provider.Id,
                actorUserId,
                Before: new { IsActive = before },
                After: new { provider.IsActive }
            ),
            cancellationToken
        );
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(provider);
    }

    private async Task<Provider> GetTrackedAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        return await dbContext.Providers.FirstOrDefaultAsync(
            x => x.Id == id && !x.IsDeleted,
            cancellationToken
        ) ?? throw new KeyNotFoundException("El proveedor de almacenamiento no existe.");
    }

    private async Task UnsetCurrentDefaultAsync(
        Guid? actorUserId,
        CancellationToken cancellationToken,
        Guid? exceptProviderId = null
    )
    {
        var defaults = await dbContext
            .Providers.Where(x =>
                x.IsDefault
                && !x.IsDeleted
                && (!exceptProviderId.HasValue || x.Id != exceptProviderId.Value)
            )
            .ToListAsync(cancellationToken);

        foreach (var current in defaults)
        {
            current.Update(
                current.Name,
                current.Configuration,
                isDefault: false,
                actorUserId
            );
        }
    }

    private static ProviderDto ToDto(Provider provider)
    {
        return new ProviderDto(
            provider.Id,
            provider.Code,
            provider.Name,
            provider.ProviderType.ToString(),
            provider.IsDefault,
            provider.IsActive,
            provider.Configuration
        );
    }
}

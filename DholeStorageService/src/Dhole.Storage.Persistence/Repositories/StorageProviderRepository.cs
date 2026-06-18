using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Contracts.Providers;
using Dhole.Storage.Domain.Providers.Entities;
using Dhole.Storage.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Storage.Persistence.Repositories;

public sealed class StorageProviderRepository(ServiceDbContext dbContext)
    : EfRepository<StorageProvider, Guid>(dbContext),
        IStorageProviderRepository
{
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var value = code.Trim();

        return dbContext.StorageProviders.AnyAsync(
            x => x.Code == value && !x.IsDeleted,
            cancellationToken
        );
    }

    public Task<StorageProvider?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var value = code.Trim();

        return dbContext.StorageProviders.FirstOrDefaultAsync(
            x => x.Code == value && !x.IsDeleted,
            cancellationToken
        );
    }

    public Task<StorageProvider?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.StorageProviders.FirstOrDefaultAsync(
            x => x.IsDefault && x.IsActive && !x.IsDeleted,
            cancellationToken
        );
    }

    public async Task<PagedResult<StorageProviderDto>> GetPagedAsync(
        PageRequest page,
        string? search = null,
        string? providerType = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext.StorageProviders.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(providerType))
        {
            var value = providerType.Trim();
            query = query.Where(x => x.ProviderType == value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();

            query = query.Where(x =>
                x.Code.ToLower().Contains(value)
                || x.Name.ToLower().Contains(value)
                || x.ProviderType.ToLower().Contains(value)
            );
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new StorageProviderDto(
                x.Id,
                x.Code,
                x.Name,
                x.ProviderType,
                x.IsDefault,
                x.IsActive,
                x.Configuration
            ))
            .ToListAsync(cancellationToken);

        return PagedResult<StorageProviderDto>.Create(items, page.PageNumber, page.PageSize, total);
    }

    public async Task<IReadOnlyCollection<StorageProviderSelectDto>> GetForSelectAsync(
        string? providerType = null,
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .StorageProviders.AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive);

        if (!string.IsNullOrWhiteSpace(providerType))
        {
            var value = providerType.Trim();
            query = query.Where(x => x.ProviderType == value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();

            query = query.Where(x =>
                x.Code.ToLower().Contains(value)
                || x.Name.ToLower().Contains(value)
                || x.ProviderType.ToLower().Contains(value)
            );
        }

        return await query
            .OrderBy(x => x.Name)
            .Take(50)
            .Select(x => new StorageProviderSelectDto(x.Id, x.Code, x.Name, x.ProviderType))
            .ToListAsync(cancellationToken);
    }
}

using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Contracts.Providers.Response;
using Dhole.Storage.Domain.Providers.Entities;
using Dhole.Storage.Domain.Providers.Enums;
using Dhole.Storage.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Storage.Persistence.Repositories;

public sealed class ProviderRepository(ServiceDbContext dbContext)
    : EfRepository<Provider, Guid>(dbContext),
        IProviderRepository
{
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var value = code.Trim();

        return dbContext.Providers.AnyAsync(
            x => x.Code == value && !x.IsDeleted,
            cancellationToken
        );
    }

    public Task<Provider?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var value = code.Trim();

        return dbContext.Providers.FirstOrDefaultAsync(
            x => x.Code == value && !x.IsDeleted,
            cancellationToken
        );
    }

    public Task<Provider?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Providers.FirstOrDefaultAsync(
            x => x.IsDefault && x.IsActive && !x.IsDeleted,
            cancellationToken
        );
    }

    public async Task<PagedResult<ProviderDto>> GetPagedAsync(
        PageRequest page,
        string? search = null,
        string? providerType = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext.Providers.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(providerType))
        {
            query = Enum.TryParse<ProviderType>(providerType.Trim(), true, out var parsedType)
                ? query.Where(x => x.ProviderType == parsedType)
                : query.Where(_ => false);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLowerInvariant();
            var matchesProviderType = Enum.TryParse<ProviderType>(search.Trim(), true, out var searchedType);

            query = query.Where(x =>
                x.Code.ToLower().Contains(value)
                || x.Name.ToLower().Contains(value)
                || (matchesProviderType && x.ProviderType == searchedType)
            );
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var providers = await query
            .OrderBy(x => x.Name)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);
        var items = providers.Select(ToDto).ToArray();

        return PagedResult<ProviderDto>.Create(items, page.PageNumber, page.PageSize, total);
    }

    public async Task<IReadOnlyCollection<ProviderSelectDto>> GetForSelectAsync(
        string? providerType = null,
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext.Providers.AsNoTracking().Where(x => !x.IsDeleted && x.IsActive);

        if (!string.IsNullOrWhiteSpace(providerType))
        {
            query = Enum.TryParse<ProviderType>(providerType.Trim(), true, out var parsedType)
                ? query.Where(x => x.ProviderType == parsedType)
                : query.Where(_ => false);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLowerInvariant();
            var matchesProviderType = Enum.TryParse<ProviderType>(search.Trim(), true, out var searchedType);

            query = query.Where(x =>
                x.Code.ToLower().Contains(value)
                || x.Name.ToLower().Contains(value)
                || (matchesProviderType && x.ProviderType == searchedType)
            );
        }

        var providers = await query
            .OrderBy(x => x.Name)
            .Take(50)
            .ToListAsync(cancellationToken);

        return providers
            .Select(x => new ProviderSelectDto(
                x.Id,
                x.Code,
                x.Name,
                x.ProviderType.ToString()
            ))
            .ToArray();
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

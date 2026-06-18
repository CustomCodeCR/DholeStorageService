using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Contracts.Files;
using Dhole.Storage.Domain.Files.Entities;
using Dhole.Storage.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Storage.Persistence.Repositories;

public sealed class StorageFileRepository(ServiceDbContext dbContext)
    : EfRepository<StorageFile, Guid>(dbContext),
        IStorageFileRepository
{
    public Task<StorageFile?> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .Files.Include(x => x.Versions)
            .Include(x => x.References)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<StorageFileDto>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default
    )
    {
        var value = entityType.Trim();

        return await dbContext
            .Files.AsNoTracking()
            .Include(x => x.Versions)
            .Include(x => x.References)
            .Where(x =>
                !x.IsDeleted
                && x.References.Any(r => r.EntityType == value && r.EntityId == entityId)
            )
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<StorageFileVersionDto>> GetVersionsAsync(
        Guid fileId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .FileVersions.AsNoTracking()
            .Where(x => x.FileId == fileId)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new StorageFileVersionDto(
                x.Id,
                x.VersionNumber,
                x.StoredFileName,
                x.StoragePath,
                x.SizeInBytes,
                x.Checksum,
                x.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<StorageFileDto>> GetPagedAsync(
        PageRequest page,
        string? sourceService = null,
        string? entityType = null,
        Guid? entityId = null,
        string? documentType = null,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .Files.AsNoTracking()
            .Include(x => x.Versions)
            .Include(x => x.References)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var value = status.Trim();
            query = query.Where(x => x.Status == value);
        }

        if (!string.IsNullOrWhiteSpace(sourceService))
        {
            var value = sourceService.Trim();
            query = query.Where(x => x.References.Any(r => r.SourceService == value));
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            var value = entityType.Trim();
            query = query.Where(x => x.References.Any(r => r.EntityType == value));
        }

        if (entityId.HasValue)
        {
            query = query.Where(x => x.References.Any(r => r.EntityId == entityId.Value));
        }

        if (!string.IsNullOrWhiteSpace(documentType))
        {
            var value = documentType.Trim();
            query = query.Where(x => x.References.Any(r => r.ReferenceType == value));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();

            query = query.Where(x =>
                x.OriginalFileName.ToLower().Contains(value)
                || x.StoredFileName.ToLower().Contains(value)
                || x.ContentType.ToLower().Contains(value)
                || (x.Extension != null && x.Extension.ToLower().Contains(value))
            );
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return PagedResult<StorageFileDto>.Create(items, page.PageNumber, page.PageSize, total);
    }

    public async Task<IReadOnlyCollection<StorageFileSelectDto>> GetForSelectAsync(
        string? sourceService = null,
        string? entityType = null,
        Guid? entityId = null,
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .Files.AsNoTracking()
            .Include(x => x.References)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(sourceService))
        {
            var value = sourceService.Trim();
            query = query.Where(x => x.References.Any(r => r.SourceService == value));
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            var value = entityType.Trim();
            query = query.Where(x => x.References.Any(r => r.EntityType == value));
        }

        if (entityId.HasValue)
        {
            query = query.Where(x => x.References.Any(r => r.EntityId == entityId.Value));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();

            query = query.Where(x =>
                x.OriginalFileName.ToLower().Contains(value)
                || x.StoredFileName.ToLower().Contains(value)
                || x.ContentType.ToLower().Contains(value)
                || (x.Extension != null && x.Extension.ToLower().Contains(value))
            );
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(50)
            .Select(x => new StorageFileSelectDto(
                x.Id,
                x.OriginalFileName,
                x.ContentType,
                x.SizeInBytes,
                x.Status,
                x.CurrentVersionNumber
            ))
            .ToListAsync(cancellationToken);
    }

    private static StorageFileDto ToDto(StorageFile file)
    {
        return new StorageFileDto(
            file.Id,
            file.ProviderId,
            file.OriginalFileName,
            file.StoredFileName,
            file.ContentType,
            file.Extension,
            file.SizeInBytes,
            file.StoragePath,
            file.Checksum,
            file.Status,
            file.CurrentVersionNumber,
            file.MetadataJson,
            file.CreatedAtUtc,
            file.References.Select(x => new StorageFileReferenceDto(
                    x.Id,
                    x.SourceService,
                    x.EntityType,
                    x.EntityId,
                    x.ReferenceType
                ))
                .ToList(),
            file.Versions.OrderByDescending(x => x.VersionNumber)
                .Select(x => new StorageFileVersionDto(
                    x.Id,
                    x.VersionNumber,
                    x.StoredFileName,
                    x.StoragePath,
                    x.SizeInBytes,
                    x.Checksum,
                    x.CreatedAtUtc
                ))
                .ToList()
        );
    }
}

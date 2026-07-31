using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Contracts.Files.Response;
using Dhole.Storage.Domain.Files.Enums;
using Dhole.Storage.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Storage.Persistence.Repositories;

public sealed class FileRepository(ServiceDbContext dbContext)
    : EfRepository<Dhole.Storage.Domain.Files.Entities.File, Guid>(dbContext),
        IFileRepository
{
    public Task<Dhole.Storage.Domain.Files.Entities.File?> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .Files.Include(x => x.Versions)
            .Include(x => x.References)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<FileDto>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default
    )
    {
        var value = entityType.Trim();

        var files = await dbContext
            .Files.AsNoTracking()
            .Include(x => x.Versions)
            .Include(x => x.References)
            .Where(x =>
                !x.IsDeleted
                && x.References.Any(r => r.EntityType == value && r.EntityId == entityId)
            )
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return files.Select(ToDto).ToArray();
    }

    public async Task<IReadOnlyCollection<FileVersionDto>> GetVersionsAsync(
        Guid fileId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .FileVersions.AsNoTracking()
            .Where(x => x.FileId == fileId)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new FileVersionDto(
                x.Id,
                x.VersionNumber,
                x.StoredFileName,
                x.Path,
                x.SizeInBytes,
                x.Checksum,
                x.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<FileDto>> GetPagedAsync(
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
            query = Enum.TryParse<FileStatus>(status.Trim(), true, out var parsedStatus)
                ? query.Where(x => x.Status == parsedStatus)
                : query.Where(_ => false);
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

        var entities = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);
        var items = entities.Select(ToDto).ToArray();

        return PagedResult<FileDto>.Create(items, page.PageNumber, page.PageSize, total);
    }

    public async Task<IReadOnlyCollection<FileSelectDto>> GetForSelectAsync(
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

        var files = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        return files.Select(x => new FileSelectDto(
                x.Id,
                x.OriginalFileName,
                x.StoredFileName,
                x.SizeInBytes,
                x.Status.ToString(),
                x.CurrentVersionNumber
            ))
            .ToArray();
    }

    private static FileDto ToDto(Dhole.Storage.Domain.Files.Entities.File file)
    {
        return new FileDto(
            file.Id,
            file.ProviderId,
            file.OriginalFileName,
            file.StoredFileName,
            file.ContentType,
            file.Extension,
            file.SizeInBytes,
            file.Path,
            file.Checksum,
            file.Status.ToString(),
            file.CurrentVersionNumber,
            file.MetadataJson,
            file.CreatedAtUtc,
            file.References.Select(x => new FileReferenceDto(
                    x.Id,
                    x.SourceService,
                    x.EntityType,
                    x.EntityId
                ))
                .ToList(),
            file.Versions.OrderByDescending(x => x.VersionNumber)
                .Select(x => new FileVersionDto(
                    x.Id,
                    x.VersionNumber,
                    x.StoredFileName,
                    x.Path,
                    x.SizeInBytes,
                    x.Checksum,
                    x.CreatedAtUtc
                ))
                .ToList()
        );
    }
}

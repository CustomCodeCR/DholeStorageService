using System.Text.Json;
using Dhole.Storage.Application.Abstractions.Auditing;
using Dhole.Storage.Application.Abstractions.Services;
using Dhole.Storage.Application.Abstractions.Storage;
using Dhole.Storage.Contracts.Files.Response;
using Dhole.Storage.Domain.Files.Enums;
using Dhole.Storage.Domain.Providers.Entities;
using Dhole.Storage.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using StorageFile = Dhole.Storage.Domain.Files.Entities.File;

namespace Dhole.Storage.Api.Services;

public sealed class StorageFileApplicationService(
    ServiceDbContext dbContext,
    IStorageObjectStoreResolver objectStoreResolver,
    IFileNameGenerator fileNameGenerator,
    IPathResolver pathResolver,
    IChecksumService checksumService,
    IStorageAuditService auditService,
    IConfiguration configuration,
    ILogger<StorageFileApplicationService> logger
)
{
    private readonly long _maximumFileSizeBytes = ReadPositiveLong(
        configuration["Storage:MaximumFileSizeBytes"],
        100L * 1024L * 1024L
    );

    public async Task<StoredFileResponse> UploadAsync(
        IFormFile formFile,
        string sourceService,
        string entityType,
        Guid entityId,
        Guid? providerId,
        string? metadataJson,
        Guid? actorUserId,
        CancellationToken cancellationToken
    )
    {
        ValidateReference(sourceService, entityType, entityId);
        ValidateFile(formFile);
        ValidateMetadata(metadataJson);

        var provider = await ResolveProviderAsync(providerId, cancellationToken);
        var bytes = await ReadFormFileAsync(formFile, cancellationToken);
        await using var contentStream = new MemoryStream(bytes, writable: false);
        var checksum = await checksumService.CalculateAsync(contentStream, cancellationToken);

        var existing = await dbContext
            .Files.AsNoTracking()
            .Include(x => x.References)
            .FirstOrDefaultAsync(
                x =>
                    !x.IsDeleted
                    && x.Status == FileStatus.Uploaded
                    && x.Checksum == checksum
                    && x.References.Any(r =>
                        r.SourceService == sourceService
                        && r.EntityType == entityType
                        && r.EntityId == entityId
                    ),
                cancellationToken
            );

        if (existing is not null)
        {
            var existingProvider = existing.ProviderId == provider.Id
                ? provider
                : await dbContext.Providers.AsNoTracking().FirstAsync(
                    x => x.Id == existing.ProviderId,
                    cancellationToken
                );
            return ToStoredResponse(existing, existingProvider);
        }

        var extension = Path.GetExtension(formFile.FileName);
        var storedFileName = fileNameGenerator.GenerateStoredFileName(
            formFile.FileName,
            extension
        );
        var storagePath = pathResolver.ResolveFilePath(
            sourceService,
            entityType,
            entityId,
            storedFileName
        );
        var contentType = string.IsNullOrWhiteSpace(formFile.ContentType)
            ? "application/octet-stream"
            : formFile.ContentType.Trim();
        var objectStore = objectStoreResolver.Resolve(provider.ProviderType);

        await using var uploadStream = new MemoryStream(bytes, writable: false);
        await objectStore.WriteAsync(
            storagePath,
            uploadStream,
            contentType,
            provider.Configuration,
            cancellationToken
        );

        try
        {
            var file = StorageFile.Upload(
                provider.Id,
                sourceService.Trim(),
                entityType.Trim(),
                entityId,
                Path.GetFileName(formFile.FileName),
                storedFileName,
                contentType,
                string.IsNullOrWhiteSpace(extension) ? null : extension.ToLowerInvariant(),
                bytes.LongLength,
                storagePath,
                checksum,
                metadataJson,
                actorUserId
            );

            dbContext.Files.Add(file);
            await auditService.PublishAsync(
                new StorageAuditEvent(
                    "storage.file.uploaded",
                    "Upload",
                    "File",
                    file.Id,
                    actorUserId,
                    After: new
                    {
                        file.Id,
                        file.ProviderId,
                        SourceService = sourceService,
                        EntityType = entityType,
                        EntityId = entityId,
                        file.OriginalFileName,
                        file.ContentType,
                        file.SizeInBytes,
                        file.Checksum,
                        file.Path,
                        file.MetadataJson,
                    }
                ),
                cancellationToken
            );
            await dbContext.SaveChangesAsync(cancellationToken);

            return ToStoredResponse(file, provider);
        }
        catch
        {
            await TryDeletePhysicalObjectAsync(
                objectStore,
                storagePath,
                provider.Configuration,
                cancellationToken
            );
            throw;
        }
    }

    public async Task<StoredFileResponse> UploadVersionAsync(
        Guid fileId,
        IFormFile formFile,
        Guid? actorUserId,
        CancellationToken cancellationToken
    )
    {
        ValidateFile(formFile);

        var file = await GetFileWithDetailsAsync(fileId, cancellationToken);
        var provider = await dbContext.Providers.FirstOrDefaultAsync(
            x => x.Id == file.ProviderId && !x.IsDeleted,
            cancellationToken
        ) ?? throw new KeyNotFoundException("El proveedor del archivo no existe.");

        if (!provider.IsActive)
        {
            throw new InvalidOperationException("El proveedor de almacenamiento está inactivo.");
        }

        var reference = file.References.FirstOrDefault()
            ?? throw new InvalidOperationException("El archivo no tiene una referencia de origen.");
        var bytes = await ReadFormFileAsync(formFile, cancellationToken);
        await using var checksumStream = new MemoryStream(bytes, writable: false);
        var checksum = await checksumService.CalculateAsync(checksumStream, cancellationToken);
        var nextVersion = file.Versions.Count == 0
            ? 1
            : file.Versions.Max(x => x.VersionNumber) + 1;
        var storedFileName = fileNameGenerator.GenerateVersionedStoredFileName(
            file.Id,
            nextVersion,
            formFile.FileName,
            Path.GetExtension(formFile.FileName)
        );
        var storagePath = pathResolver.ResolveVersionPath(
            reference.SourceService,
            reference.EntityType,
            reference.EntityId,
            file.Id,
            nextVersion,
            storedFileName
        );
        var contentType = string.IsNullOrWhiteSpace(formFile.ContentType)
            ? file.ContentType
            : formFile.ContentType.Trim();
        var objectStore = objectStoreResolver.Resolve(provider.ProviderType);

        await using var uploadStream = new MemoryStream(bytes, writable: false);
        await objectStore.WriteAsync(
            storagePath,
            uploadStream,
            contentType,
            provider.Configuration,
            cancellationToken
        );

        try
        {
            var version = file.AddVersion(
                storedFileName,
                storagePath,
                bytes.LongLength,
                checksum,
                actorUserId
            );
            file.SetCurrentVersion(version.VersionNumber, actorUserId);

            await auditService.PublishAsync(
                new StorageAuditEvent(
                    "storage.file.version-uploaded",
                    "UploadVersion",
                    "File",
                    file.Id,
                    actorUserId,
                    After: new
                    {
                        FileId = file.Id,
                        VersionId = version.Id,
                        version.VersionNumber,
                        version.StoredFileName,
                        version.SizeInBytes,
                        version.Checksum,
                        version.Path,
                    }
                ),
                cancellationToken
            );
            await dbContext.SaveChangesAsync(cancellationToken);
            return ToStoredResponse(file, provider);
        }
        catch
        {
            await TryDeletePhysicalObjectAsync(
                objectStore,
                storagePath,
                provider.Configuration,
                cancellationToken
            );
            throw;
        }
    }

    public async Task<BrowseStorageFilesResponse> BrowseAsync(
        int pageNumber,
        int pageSize,
        string? search,
        string? contentType,
        string? sourceService,
        string? entityType,
        Guid? providerId,
        CancellationToken cancellationToken
    )
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 100);
        var query = dbContext.Files.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var rawSearch = search.Trim();
            var term = rawSearch.ToLowerInvariant();
            var hasEntityId = Guid.TryParse(rawSearch, out var entityIdSearch);
            query = query.Where(x =>
                x.OriginalFileName.ToLower().Contains(term)
                || x.StoredFileName.ToLower().Contains(term)
                || (x.Checksum != null && x.Checksum.ToLower().Contains(term))
                || x.References.Any(r =>
                    r.SourceService.ToLower().Contains(term)
                    || r.EntityType.ToLower().Contains(term)
                    || (hasEntityId && r.EntityId == entityIdSearch)
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            var normalizedContentType = contentType.Trim().ToLowerInvariant();
            query = query.Where(x => x.ContentType.ToLower().Contains(normalizedContentType));
        }

        if (!string.IsNullOrWhiteSpace(sourceService))
        {
            var normalizedSource = sourceService.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.References.Any(r => r.SourceService.ToLower() == normalizedSource)
            );
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            var normalizedEntityType = entityType.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.References.Any(r => r.EntityType.ToLower() == normalizedEntityType)
            );
        }

        if (providerId.HasValue)
        {
            query = query.Where(x => x.ProviderId == providerId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var files = await query
            .Include(x => x.References)
            .Include(x => x.Versions)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        var providerIds = files.Select(x => x.ProviderId).Distinct().ToArray();
        var providers = await dbContext.Providers.AsNoTracking()
            .Where(x => providerIds.Contains(x.Id) && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var items = files.Select(file =>
        {
            providers.TryGetValue(file.ProviderId, out var provider);
            var reference = file.References.FirstOrDefault();

            return new StorageFileListItemDto(
                file.Id,
                file.ProviderId,
                provider?.Name ?? "Proveedor no disponible",
                provider?.ProviderType.ToString() ?? "Unknown",
                file.OriginalFileName,
                file.ContentType,
                file.Extension,
                file.SizeInBytes,
                file.Checksum,
                file.Status.ToString(),
                file.CurrentVersionNumber,
                file.CreatedAtUtc,
                reference?.SourceService,
                reference?.EntityType,
                reference?.EntityId,
                file.References.Count,
                file.Versions.Count,
                file.MetadataJson
            );
        }).ToArray();

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)size);
        return new BrowseStorageFilesResponse(items, totalCount, page, size, totalPages);
    }

    public async Task<StorageSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var files = dbContext.Files.AsNoTracking().Where(x => !x.IsDeleted);
        var totalFiles = await files.CountAsync(cancellationToken);
        var totalSize = await files.Select(x => (long?)x.SizeInBytes).SumAsync(cancellationToken) ?? 0L;
        var imageFiles = await files.CountAsync(
            x => x.ContentType.ToLower().StartsWith("image/"),
            cancellationToken
        );
        var pdfFiles = await files.CountAsync(
            x => x.ContentType.ToLower() == "application/pdf" || x.Extension == ".pdf",
            cancellationToken
        );
        var providerCount = await dbContext.Providers.AsNoTracking()
            .CountAsync(x => !x.IsDeleted, cancellationToken);
        var activeProviderCount = await dbContext.Providers.AsNoTracking()
            .CountAsync(x => !x.IsDeleted && x.IsActive, cancellationToken);

        return new StorageSummaryDto(
            totalFiles,
            totalSize,
            imageFiles,
            pdfFiles,
            Math.Max(0, totalFiles - imageFiles - pdfFiles),
            providerCount,
            activeProviderCount
        );
    }

    public async Task<FileDto?> GetAsync(Guid fileId, CancellationToken cancellationToken)
    {
        var file = await dbContext
            .Files.AsNoTracking()
            .Include(x => x.References)
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);

        return file is null ? null : ToDto(file);
    }

    public async Task<IReadOnlyCollection<FileDto>> GetByEntityAsync(
        string sourceService,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken
    )
    {
        var source = sourceService.Trim();
        var type = entityType.Trim();

        var files = await dbContext
            .Files.AsNoTracking()
            .Include(x => x.References)
            .Include(x => x.Versions)
            .Where(x =>
                !x.IsDeleted
                && x.References.Any(r =>
                    r.SourceService == source && r.EntityType == type && r.EntityId == entityId
                )
            )
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return files.Select(ToDto).ToArray();
    }

    public async Task<DownloadFileDto> DownloadAsync(
        Guid fileId,
        CancellationToken cancellationToken
    )
    {
        var file = await GetFileWithDetailsAsync(fileId, cancellationToken);
        var provider = await dbContext.Providers.AsNoTracking().FirstOrDefaultAsync(
            x => x.Id == file.ProviderId && !x.IsDeleted,
            cancellationToken
        ) ?? throw new KeyNotFoundException("El proveedor del archivo no existe.");
        var objectStore = objectStoreResolver.Resolve(provider.ProviderType);
        var result = await objectStore.ReadAsync(
            file.Path,
            provider.Configuration,
            cancellationToken
        );

        await auditService.PublishAsync(
            new StorageAuditEvent(
                "storage.file.downloaded",
                "Download",
                "File",
                file.Id,
                Payload: new
                {
                    file.OriginalFileName,
                    file.ContentType,
                    file.SizeInBytes,
                    file.Checksum,
                }
            ),
            cancellationToken
        );
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DownloadFileDto(
            new MemoryStream(result.Content, writable: false),
            file.OriginalFileName,
            result.ContentType ?? file.ContentType,
            result.Content.LongLength
        );
    }

    public async Task ChangeCurrentVersionAsync(
        Guid fileId,
        int versionNumber,
        Guid? actorUserId,
        CancellationToken cancellationToken
    )
    {
        var file = await GetFileWithDetailsAsync(fileId, cancellationToken);
        var before = file.CurrentVersionNumber;
        file.SetCurrentVersion(versionNumber, actorUserId);

        await auditService.PublishAsync(
            new StorageAuditEvent(
                "storage.file.current-version-changed",
                "ChangeCurrentVersion",
                "File",
                file.Id,
                actorUserId,
                Before: new { VersionNumber = before },
                After: new { VersionNumber = file.CurrentVersionNumber }
            ),
            cancellationToken
        );
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid fileId,
        Guid? actorUserId,
        CancellationToken cancellationToken
    )
    {
        var file = await GetFileWithDetailsAsync(fileId, cancellationToken);
        var provider = await dbContext.Providers.AsNoTracking().FirstOrDefaultAsync(
            x => x.Id == file.ProviderId && !x.IsDeleted,
            cancellationToken
        ) ?? throw new KeyNotFoundException("El proveedor del archivo no existe.");
        var objectStore = objectStoreResolver.Resolve(provider.ProviderType);
        var paths = file.Versions.Select(x => x.Path).Append(file.Path).Distinct().ToArray();

        file.Delete(actorUserId);
        await auditService.PublishAsync(
            new StorageAuditEvent(
                "storage.file.deleted",
                "Delete",
                "File",
                file.Id,
                actorUserId,
                Before: new
                {
                    file.OriginalFileName,
                    file.ContentType,
                    file.SizeInBytes,
                    file.Checksum,
                    Paths = paths,
                }
            ),
            cancellationToken
        );
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var path in paths)
        {
            await TryDeletePhysicalObjectAsync(
                objectStore,
                path,
                provider.Configuration,
                cancellationToken
            );
        }
    }

    private async Task<Provider> ResolveProviderAsync(
        Guid? providerId,
        CancellationToken cancellationToken
    )
    {
        var provider = providerId.HasValue
            ? await dbContext.Providers.FirstOrDefaultAsync(
                x => x.Id == providerId.Value && !x.IsDeleted,
                cancellationToken
            )
            : await dbContext.Providers.FirstOrDefaultAsync(
                x => x.IsDefault && x.IsActive && !x.IsDeleted,
                cancellationToken
            );

        provider ??= await dbContext.Providers.FirstOrDefaultAsync(
            x => x.IsActive && !x.IsDeleted,
            cancellationToken
        );

        if (provider is null)
        {
            throw new InvalidOperationException(
                "No existe un proveedor de almacenamiento activo."
            );
        }

        if (!provider.IsActive)
        {
            throw new InvalidOperationException("El proveedor de almacenamiento está inactivo.");
        }

        return provider;
    }

    private async Task<StorageFile> GetFileWithDetailsAsync(
        Guid fileId,
        CancellationToken cancellationToken
    )
    {
        return await dbContext
            .Files.Include(x => x.References)
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException("El archivo no existe.");
    }

    private async Task<byte[]> ReadFormFileAsync(
        IFormFile formFile,
        CancellationToken cancellationToken
    )
    {
        if (formFile.Length > _maximumFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"El archivo excede el tamaño máximo permitido de {_maximumFileSizeBytes} bytes."
            );
        }

        await using var input = formFile.OpenReadStream();
        using var memory = new MemoryStream(
            formFile.Length > int.MaxValue ? 0 : (int)formFile.Length
        );
        await input.CopyToAsync(memory, cancellationToken);

        if (memory.Length > _maximumFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"El archivo excede el tamaño máximo permitido de {_maximumFileSizeBytes} bytes."
            );
        }

        return memory.ToArray();
    }

    private void ValidateFile(IFormFile formFile)
    {
        if (formFile is null || formFile.Length <= 0)
        {
            throw new InvalidOperationException("Debe adjuntar un archivo no vacío.");
        }

        if (formFile.Length > _maximumFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"El archivo excede el tamaño máximo permitido de {_maximumFileSizeBytes} bytes."
            );
        }
    }

    private static void ValidateReference(string sourceService, string entityType, Guid entityId)
    {
        if (string.IsNullOrWhiteSpace(sourceService))
        {
            throw new InvalidOperationException("SourceService es requerido.");
        }

        if (string.IsNullOrWhiteSpace(entityType))
        {
            throw new InvalidOperationException("EntityType es requerido.");
        }

        if (entityId == Guid.Empty)
        {
            throw new InvalidOperationException("EntityId es requerido.");
        }
    }

    private static void ValidateMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return;
        }

        try
        {
            using var _ = JsonDocument.Parse(metadataJson);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("MetadataJson debe contener JSON válido.", exception);
        }
    }

    private async Task TryDeletePhysicalObjectAsync(
        IStorageObjectStore objectStore,
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await objectStore.DeleteAsync(path, providerConfiguration, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "No se pudo eliminar el objeto físico {StoragePath} durante la compensación.",
                path
            );
        }
    }

    private static StoredFileResponse ToStoredResponse(StorageFile file, Provider provider)
    {
        return new StoredFileResponse(
            file.Id,
            $"storage://{file.Id:D}",
            file.OriginalFileName,
            file.ContentType,
            file.SizeInBytes,
            file.Checksum,
            file.CurrentVersionNumber,
            provider.Id,
            provider.ProviderType.ToString(),
            file.Path
        );
    }

    private static FileDto ToDto(StorageFile file)
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
                .ToArray(),
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
                .ToArray()
        );
    }

    private static long ReadPositiveLong(string? value, long fallback)
    {
        return long.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
    }
}

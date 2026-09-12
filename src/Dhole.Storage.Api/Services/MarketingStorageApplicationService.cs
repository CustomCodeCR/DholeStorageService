using System.Text.Json;
using System.Text.Json.Nodes;
using Dhole.Storage.Application.Abstractions.Auditing;
using Dhole.Storage.Application.Abstractions.Media;
using Dhole.Storage.Application.Abstractions.Storage;
using Dhole.Storage.Application.Media;
using Dhole.Storage.Contracts.Files.Response;
using Dhole.Storage.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Storage.Api.Services;

public sealed class MarketingStorageApplicationService(
    StorageFileApplicationService storageFiles,
    ServiceDbContext dbContext,
    IStorageObjectStoreResolver objectStoreResolver,
    IMarketingMediaProcessor mediaProcessor,
    IStorageAuditService auditService,
    IConfiguration configuration,
    ILogger<MarketingStorageApplicationService> logger)
{
    private readonly MarketingMediaLimits _limits = new(
        ReadPositiveLong(configuration["MarketingMedia:MaximumImageSizeBytes"], 25L * 1024L * 1024L),
        ReadPositiveLong(configuration["MarketingMedia:MaximumVideoSizeBytes"], 100L * 1024L * 1024L),
        ReadPositiveLong(configuration["MarketingMedia:MaximumDocumentSizeBytes"], 50L * 1024L * 1024L));

    public async Task<MarketingStoredFileResponse> UploadAsync(
        IFormFile formFile,
        string sourceService,
        string entityType,
        Guid entityId,
        Guid? providerId,
        string? metadataJson,
        Guid? actorUserId,
        CancellationToken cancellationToken)
    {
        var bytes = await ReadAsync(formFile, cancellationToken);
        var descriptor = MarketingMediaPolicy.Validate(
            formFile.FileName,
            formFile.ContentType,
            bytes.LongLength,
            bytes,
            _limits);
        var processed = await mediaProcessor.ProcessAsync(
            new MarketingMediaInput(
                formFile.FileName,
                string.IsNullOrWhiteSpace(formFile.ContentType)
                    ? descriptor.ContentType
                    : formFile.ContentType,
                bytes),
            cancellationToken);

        var stored = await storageFiles.UploadAsync(
            formFile,
            sourceService,
            entityType,
            entityId,
            providerId,
            metadataJson,
            actorUserId,
            cancellationToken);

        var writtenPaths = new List<string>();
        try
        {
            var file = await dbContext.Files.FirstAsync(x => x.Id == stored.Id, cancellationToken);
            var provider = await dbContext.Providers.AsNoTracking().FirstAsync(
                x => x.Id == file.ProviderId && !x.IsDeleted,
                cancellationToken);
            var objectStore = objectStoreResolver.Resolve(provider.ProviderType);

            var variants = new List<MarketingMediaVariantDto>();
            foreach (var asset in processed.Assets)
            {
                var path = BuildDerivedPath(file.Path, file.Id, asset.FileName);
                await using var stream = new MemoryStream(asset.Content, writable: false);
                await objectStore.WriteAsync(
                    path,
                    stream,
                    asset.ContentType,
                    provider.Configuration,
                    cancellationToken);
                writtenPaths.Add(path);
                variants.Add(new MarketingMediaVariantDto(
                    asset.Key,
                    asset.FileName,
                    asset.ContentType,
                    path,
                    asset.Content.LongLength));
            }

            var mergedMetadata = MergeMetadata(
                metadataJson,
                descriptor,
                processed,
                variants);
            file.UpdateMetadata(mergedMetadata, actorUserId);

            await auditService.PublishAsync(
                new StorageAuditEvent(
                    "storage.marketing-media.processed",
                    "ProcessMarketingMedia",
                    "File",
                    file.Id,
                    actorUserId,
                    After: new
                    {
                        Category = processed.Category,
                        processed.Format,
                        processed.Width,
                        processed.Height,
                        processed.DurationSeconds,
                        Variants = variants
                    }),
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new MarketingStoredFileResponse(
                stored,
                processed.Category,
                processed.Format,
                processed.Width,
                processed.Height,
                processed.DurationSeconds,
                variants);
        }
        catch
        {
            foreach (var path in writtenPaths)
            {
                try
                {
                    var file = await dbContext.Files.AsNoTracking().FirstOrDefaultAsync(
                        x => x.Id == stored.Id,
                        cancellationToken);
                    if (file is null) continue;
                    var provider = await dbContext.Providers.AsNoTracking().FirstOrDefaultAsync(
                        x => x.Id == file.ProviderId && !x.IsDeleted,
                        cancellationToken);
                    if (provider is null) continue;
                    var objectStore = objectStoreResolver.Resolve(provider.ProviderType);
                    await objectStore.DeleteAsync(path, provider.Configuration, cancellationToken);
                }
                catch (Exception cleanupException)
                {
                    logger.LogWarning(cleanupException, "No se pudo limpiar el derivado {Path}.", path);
                }
            }

            try
            {
                await storageFiles.DeleteAsync(stored.Id, actorUserId, cancellationToken);
            }
            catch (Exception cleanupException)
            {
                logger.LogWarning(cleanupException, "No se pudo compensar la carga {FileId}.", stored.Id);
            }
            throw;
        }
    }

    private async Task<byte[]> ReadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length <= 0)
            throw new InvalidOperationException("Debe adjuntar un archivo no vacío.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var maximum = extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".webp" or ".avif" => _limits.MaximumImageSizeBytes,
            ".mp4" or ".webm" or ".mov" => _limits.MaximumVideoSizeBytes,
            _ => _limits.MaximumDocumentSizeBytes,
        };
        if (file.Length > maximum)
            throw new InvalidOperationException($"El archivo excede el tamaño máximo permitido de {maximum} bytes.");

        await using var input = file.OpenReadStream();
        using var memory = new MemoryStream(file.Length > int.MaxValue ? 0 : (int)file.Length);
        await input.CopyToAsync(memory, cancellationToken);
        if (memory.Length > maximum)
            throw new InvalidOperationException($"El archivo excede el tamaño máximo permitido de {maximum} bytes.");
        return memory.ToArray();
    }

    private static string MergeMetadata(
        string? metadataJson,
        MarketingMediaDescriptor descriptor,
        MarketingMediaProcessingResult processed,
        IReadOnlyCollection<MarketingMediaVariantDto> variants)
    {
        JsonObject root;
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            root = new JsonObject();
        }
        else
        {
            try
            {
                root = JsonNode.Parse(metadataJson) as JsonObject
                    ?? throw new InvalidOperationException("MetadataJson debe ser un objeto JSON.");
            }
            catch (JsonException exception)
            {
                throw new InvalidOperationException("MetadataJson debe contener JSON válido.", exception);
            }
        }

        root["marketingMedia"] = new JsonObject
        {
            ["category"] = processed.Category,
            ["format"] = processed.Format,
            ["width"] = processed.Width,
            ["height"] = processed.Height,
            ["durationSeconds"] = processed.DurationSeconds,
            ["originalExtension"] = descriptor.Extension,
            ["originalContentType"] = descriptor.ContentType,
            ["variants"] = new JsonArray(variants.Select(v => (JsonNode)new JsonObject
            {
                ["key"] = v.Key,
                ["fileName"] = v.FileName,
                ["contentType"] = v.ContentType,
                ["path"] = v.Path,
                ["sizeInBytes"] = v.SizeInBytes
            }).ToArray())
        };
        return root.ToJsonString();
    }

    private static string BuildDerivedPath(string originalPath, Guid fileId, string fileName)
    {
        var normalized = originalPath.Replace('\\', '/');
        var separator = normalized.LastIndexOf('/');
        var directory = separator >= 0 ? normalized[..separator] : string.Empty;
        return string.IsNullOrWhiteSpace(directory)
            ? $"_derived/{fileId:N}/{fileName}"
            : $"{directory}/_derived/{fileId:N}/{fileName}";
    }

    private static long ReadPositiveLong(string? value, long fallback)
        => long.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
}

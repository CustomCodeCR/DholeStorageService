using CustomCodeFramework.Mongo.Abstractions;
using Dhole.Storage.Application.Abstractions.Mongo;
using Dhole.Storage.Infrastructure.Mongo.Documents;

namespace Dhole.Storage.Infrastructure.Mongo;

public sealed class StorageFileMetadataSnapshotWriter(IMongoContext mongoContext)
    : IStorageFileMetadataSnapshotWriter
{
    public Task WriteAsync(
        Guid eventId,
        string eventName,
        Guid fileId,
        Guid providerId,
        string sourceService,
        string entityType,
        Guid entityId,
        string originalFileName,
        string storedFileName,
        string contentType,
        string? extension,
        long sizeInBytes,
        string storagePath,
        string? checksum,
        int versionNumber,
        bool isCurrentVersion,
        string status,
        string? metadataJson,
        string action,
        Guid? changedBy,
        DateTime changedAtUtc,
        Guid? correlationId,
        CancellationToken cancellationToken = default
    )
    {
        var document = new StorageFileMetadataSnapshotDocument
        {
            EventId = eventId.ToString(),
            EventName = eventName,
            FileId = fileId.ToString(),
            ProviderId = providerId.ToString(),
            SourceService = sourceService,
            EntityType = entityType,
            EntityId = entityId.ToString(),
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            ContentType = contentType,
            Extension = extension,
            SizeInBytes = sizeInBytes,
            StoragePath = storagePath,
            Checksum = checksum,
            VersionNumber = versionNumber,
            IsCurrentVersion = isCurrentVersion,
            Status = status,
            MetadataJson = metadataJson,
            Action = action,
            ChangedBy = changedBy?.ToString(),
            ChangedAtUtc = changedAtUtc,
            CorrelationId = correlationId?.ToString(),
        };

        return mongoContext
            .GetCollection<StorageFileMetadataSnapshotDocument>()
            .InsertOneAsync(document, cancellationToken: cancellationToken);
    }
}

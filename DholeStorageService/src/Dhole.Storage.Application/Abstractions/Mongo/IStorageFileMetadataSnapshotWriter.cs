namespace Dhole.Storage.Application.Abstractions.Mongo;

public interface IStorageFileMetadataSnapshotWriter
{
    Task WriteAsync(
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
    );
}

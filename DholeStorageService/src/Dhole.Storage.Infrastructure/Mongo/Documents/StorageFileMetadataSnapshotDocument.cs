using CustomCodeFramework.Mongo.Abstractions;
using CustomCodeFramework.Mongo.Collections;

namespace Dhole.Storage.Infrastructure.Mongo.Documents;

[MongoCollectionName("storage_file_metadata_snapshots")]
public sealed class StorageFileMetadataSnapshotDocument : IReadModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public string EventId { get; init; } = default!;

    public string EventName { get; init; } = default!;

    public string FileId { get; init; } = default!;

    public string ProviderId { get; init; } = default!;

    public string SourceService { get; init; } = default!;

    public string EntityType { get; init; } = default!;

    public string EntityId { get; init; } = default!;

    public string OriginalFileName { get; init; } = default!;

    public string StoredFileName { get; init; } = default!;

    public string ContentType { get; init; } = default!;

    public string? Extension { get; init; }

    public long SizeInBytes { get; init; }

    public string StoragePath { get; init; } = default!;

    public string? Checksum { get; init; }

    public int VersionNumber { get; init; }

    public bool IsCurrentVersion { get; init; }

    public string Status { get; init; } = default!;

    public string? MetadataJson { get; init; }

    public string Action { get; init; } = default!;

    public string? ChangedBy { get; init; }

    public DateTime ChangedAtUtc { get; init; }

    public string? CorrelationId { get; init; }

    public string ServiceName { get; init; } = "DholeStorageService";
}

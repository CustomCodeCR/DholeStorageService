using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Files.Events;

public sealed record StorageFileUploadedDomainEvent(
    Guid FileId,
    Guid ProviderId,
    string SourceService,
    string EntityType,
    Guid EntityId,
    string OriginalFileName,
    string StoredFileName,
    string ContentType,
    long SizeInBytes,
    string StoragePath,
    string? Checksum,
    int VersionNumber,
    Guid? UploadedBy
) : DomainEvent;

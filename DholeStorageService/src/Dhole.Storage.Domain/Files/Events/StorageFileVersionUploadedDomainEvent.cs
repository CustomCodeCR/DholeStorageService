using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Files.Events;

public sealed record StorageFileVersionUploadedDomainEvent(
    Guid FileId,
    Guid VersionId,
    int VersionNumber,
    string OriginalFileName,
    string StoredFileName,
    string StoragePath,
    long SizeInBytes,
    string? Checksum,
    Guid? UploadedBy
) : DomainEvent;

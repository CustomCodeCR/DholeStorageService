using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Files.Events;

public sealed record FileVersionUploadedDomainEvent(
    Guid fileId,
    Guid versionId,
    int versionNumber,
    string originalFileName,
    string storedFileName,
    string path,
    long sizeInBytes,
    string? checksum,
    Guid? uploadedBy
) : DomainEvent;

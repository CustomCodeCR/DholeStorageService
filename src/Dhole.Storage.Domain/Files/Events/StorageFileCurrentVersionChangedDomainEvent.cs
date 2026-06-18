using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Files.Events;

public sealed record StorageFileCurrentVersionChangedDomainEvent(
    Guid FileId,
    Guid VersionId,
    int VersionNumber,
    Guid? ChangedBy
) : DomainEvent;

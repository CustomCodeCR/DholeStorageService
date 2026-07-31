using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Files.Events;

public sealed record FileCurrentVersionChangedDomainEvent(
    Guid fileId,
    Guid versionId,
    int versionNumber,
    Guid? changedBy
) : DomainEvent;

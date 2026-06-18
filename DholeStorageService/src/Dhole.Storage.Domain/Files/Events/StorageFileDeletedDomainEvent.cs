using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Files.Events;

public sealed record StorageFileDeletedDomainEvent(
    Guid FileId,
    string OriginalFileName,
    string SourceService,
    string EntityType,
    Guid EntityId,
    Guid? DeletedBy
) : DomainEvent;

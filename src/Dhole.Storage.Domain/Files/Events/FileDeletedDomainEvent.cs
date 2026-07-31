using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Files.Events;

public sealed record FileDeletedDomainEvent(
    Guid id,
    Guid provider,
    string orginalFileName,
    string storedFileName,
    string? extension,
    Guid? deletedBy
) : DomainEvent;

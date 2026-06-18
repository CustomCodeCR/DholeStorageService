using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Providers.Events;

public sealed record StorageProviderInactivatedDomainEvent(
    Guid ProviderId,
    string Code,
    Guid? InactivatedBy
) : DomainEvent;

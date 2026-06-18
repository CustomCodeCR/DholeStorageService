using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Providers.Events;

public sealed record StorageProviderActivatedDomainEvent(
    Guid ProviderId,
    string Code,
    Guid? ActivatedBy
) : DomainEvent;

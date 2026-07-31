using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Providers.Events;

public sealed record ProviderActivatedDomainEvent(Guid id, string code, Guid? activatedBy)
    : DomainEvent;

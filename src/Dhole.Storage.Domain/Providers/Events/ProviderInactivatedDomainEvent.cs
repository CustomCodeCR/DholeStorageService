using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Providers.Events;

public sealed record ProviderInactivatedDomainEvent(Guid id, string code, Guid? inactivatedBy)
    : DomainEvent;

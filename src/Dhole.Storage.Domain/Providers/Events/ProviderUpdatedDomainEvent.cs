using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Providers.Events;

public sealed record ProviderUpdatedDomainEvent(
    Guid id,
    string code,
    string name,
    string providerType,
    Guid? updatedBy
) : DomainEvent;

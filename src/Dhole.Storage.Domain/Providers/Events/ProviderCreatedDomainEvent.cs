using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Providers.Events;

public sealed record ProviderCreatedDomainEvent(
    Guid id,
    string code,
    string name,
    string providerType,
    Guid? createdBy
) : DomainEvent;

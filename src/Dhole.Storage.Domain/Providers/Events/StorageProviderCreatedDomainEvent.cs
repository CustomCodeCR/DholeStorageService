using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Storage.Domain.Providers.Events;

public sealed record StorageProviderCreatedDomainEvent(
    Guid ProviderId,
    string Code,
    string Name,
    string ProviderType,
    Guid? CreatedBy
) : DomainEvent;

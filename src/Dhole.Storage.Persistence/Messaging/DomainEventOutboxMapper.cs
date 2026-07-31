using CustomCodeFramework.Core.Domain.Events;
using Dhole.Storage.Domain.Files.Events;
using Dhole.Storage.Domain.Providers.Events;

namespace Dhole.Storage.Persistence.Messaging;

internal static class DomainEventOutboxMapper
{
    public static string GetEventName(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            FileCurrentVersionChangedDomainEvent => "storage.file.current-version-changed",
            FileDeletedDomainEvent => "storage.file.deleted",
            FileUploadedDomainEvent => "storage.file.uploaded",
            FileVersionUploadedDomainEvent => "storage.file.version-uploaded",

            ProviderActivatedDomainEvent => "storage.provider.activated",
            ProviderCreatedDomainEvent => "storage.provider.created",
            ProviderInactivatedDomainEvent => "storage.provider.inactivated",
            ProviderUpdatedDomainEvent => "storage.provider.updated",

            _ => $"storage.{domainEvent.GetType().Name}",
        };
    }

    public static string GetEventType(IDomainEvent domainEvent)
    {
        return domainEvent.GetType().FullName ?? domainEvent.GetType().Name;
    }
}

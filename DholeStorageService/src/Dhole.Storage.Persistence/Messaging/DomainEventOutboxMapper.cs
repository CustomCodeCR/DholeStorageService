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
            // Providers
            StorageProviderCreatedDomainEvent => "storage.provider.created",
            StorageProviderUpdatedDomainEvent => "storage.provider.updated",
            StorageProviderActivatedDomainEvent => "storage.provider.activated",
            StorageProviderInactivatedDomainEvent => "storage.provider.inactivated",

            // Files
            StorageFileUploadedDomainEvent => "storage.file.uploaded",
            StorageFileVersionUploadedDomainEvent => "storage.file.version_uploaded",
            StorageFileCurrentVersionChangedDomainEvent => "storage.file.current_version_changed",
            StorageFileDeletedDomainEvent => "storage.file.deleted",

            _ => $"storage.{domainEvent.GetType().Name}",
        };
    }

    public static string GetEventType(IDomainEvent domainEvent)
    {
        return domainEvent.GetType().FullName ?? domainEvent.GetType().Name;
    }
}

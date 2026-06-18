using System.Text.Json;
using CustomCodeFramework.Core.Domain.Entities;
using CustomCodeFramework.Messaging.Inbox;
using CustomCodeFramework.Messaging.Outbox;
using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using CustomCodeFramework.Postgres.EntityFramework.DbContexts;
using Dhole.Storage.Domain.Files.Entities;
using Dhole.Storage.Domain.Providers.Entities;
using Dhole.Storage.Persistence.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Storage.Persistence.DbContexts;

public sealed class ServiceDbContext(DbContextOptions<ServiceDbContext> options)
    : AppDbContextBase(options)
{
    public DbSet<StorageProvider> StorageProviders => Set<StorageProvider>();
    public DbSet<StorageFile> Files => Set<StorageFile>();
    public DbSet<StorageFileVersion> FileVersions => Set<StorageFileVersion>();
    public DbSet<StorageFileReference> FileReferences => Set<StorageFileReference>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddDomainEventsToOutbox();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        AddDomainEventsToOutbox();
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("storage");

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDbContext).Assembly);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
    }

    private void AddDomainEventsToOutbox()
    {
        var aggregateRoots = ChangeTracker
            .Entries()
            .Select(x => x.Entity)
            .OfType<AggregateRoot<Guid>>()
            .Where(x => x.DomainEvents.Count > 0)
            .ToList();

        if (aggregateRoots.Count == 0)
        {
            return;
        }

        var domainEvents = aggregateRoots.SelectMany(x => x.DomainEvents).ToList();

        var outboxMessages = new List<OutboxMessage>();

        foreach (var domainEvent in domainEvents)
        {
            var eventType = DomainEventOutboxMapper.GetEventType(domainEvent);
            var eventName = DomainEventOutboxMapper.GetEventName(domainEvent);
            var payloadJson = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

            outboxMessages.Add(
                new OutboxMessage
                {
                    EventId = domainEvent.EventId,
                    EventType = eventType,
                    EventName = eventName,
                    SourceService = "DholeStorageService",
                    PayloadJson = payloadJson,
                    HeadersJson = null,
                    CorrelationId = null,
                    Status = OutboxMessageStatus.Pending,
                    RetryCount = 0,
                    ErrorMessage = null,
                    CreatedAtUtc = DateTime.UtcNow,
                }
            );

            outboxMessages.Add(
                CreateAuditOutboxMessage(
                    domainEvent.EventId,
                    eventName,
                    payloadJson,
                    "DholeStorageService",
                    ResolveEntityType(eventName),
                    ResolveEntityId(domainEvent),
                    ResolveAction(eventName),
                    ResolveUserId(domainEvent)
                )
            );
        }

        OutboxMessages.AddRange(outboxMessages);

        foreach (var aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }
    }

    private static OutboxMessage CreateAuditOutboxMessage(
        Guid originalEventId,
        string eventName,
        string payloadJson,
        string sourceService,
        string? entityType,
        Guid? entityId,
        string action,
        Guid? userId
    )
    {
        var auditPayload = new
        {
            EventId = originalEventId,
            CorrelationId = Guid.NewGuid(),
            SourceService = sourceService,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            EventType = eventName,
            UserId = userId,
            UserName = (string?)null,
            IpAddress = (string?)null,
            UserAgent = (string?)null,
            OccurredAt = DateTime.UtcNow,
            BeforeJson = (string?)null,
            AfterJson = (string?)null,
            PayloadJson = payloadJson,
            Metadata = ResolveMetadata(domainEventPayloadJson: payloadJson),
            ErrorMessage = (string?)null,
            StackTrace = (string?)null,
            Details = Array.Empty<object>(),
        };

        return new OutboxMessage
        {
            EventId = Guid.NewGuid(),
            EventType = "Dhole.AuditLogs.Contracts.AuditEvents.RegisterAuditEventRequest",
            EventName = "audit.event.registered",
            SourceService = sourceService,
            PayloadJson = JsonSerializer.Serialize(auditPayload),
            HeadersJson = null,
            CorrelationId = null,
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0,
            ErrorMessage = null,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    private static string? ResolveEntityType(string eventName)
    {
        if (eventName.Contains("provider"))
            return "StorageProvider";
        if (eventName.Contains("file"))
            return "StorageFile";

        return null;
    }

    private static string ResolveAction(string eventName)
    {
        if (eventName.EndsWith(".created"))
            return "created";
        if (eventName.EndsWith(".updated"))
            return "updated";
        if (eventName.EndsWith(".activated"))
            return "activated";
        if (eventName.EndsWith(".inactivated"))
            return "inactivated";
        if (eventName.EndsWith(".uploaded"))
            return "uploaded";
        if (eventName.Contains("version_uploaded"))
            return "version_uploaded";
        if (eventName.Contains("current_version_changed"))
            return "version_changed";
        if (eventName.EndsWith(".deleted"))
            return "deleted";

        return "unknown";
    }

    private static Guid? ResolveEntityId(object domainEvent)
    {
        var property = domainEvent
            .GetType()
            .GetProperties()
            .FirstOrDefault(x =>
                x.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
                && x.PropertyType == typeof(Guid)
            );

        return property?.GetValue(domainEvent) is Guid value ? value : null;
    }

    private static Guid? ResolveUserId(object domainEvent)
    {
        var property = domainEvent
            .GetType()
            .GetProperties()
            .FirstOrDefault(x =>
                x.PropertyType == typeof(Guid?)
                && (
                    x.Name.EndsWith("By", StringComparison.OrdinalIgnoreCase)
                    || x.Name.Contains("User", StringComparison.OrdinalIgnoreCase)
                    || x.Name.Contains("Uploaded", StringComparison.OrdinalIgnoreCase)
                    || x.Name.Contains("Changed", StringComparison.OrdinalIgnoreCase)
                    || x.Name.Contains("Deleted", StringComparison.OrdinalIgnoreCase)
                )
            );

        return property?.GetValue(domainEvent) as Guid?;
    }

    private static string? ResolveMetadata(string domainEventPayloadJson)
    {
        try
        {
            using var document = JsonDocument.Parse(domainEventPayloadJson);

            if (document.RootElement.TryGetProperty("MetadataJson", out var metadata))
            {
                return metadata.ValueKind == JsonValueKind.Null ? null : metadata.GetString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}

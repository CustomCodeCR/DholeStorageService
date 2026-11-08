using System.Text.Json;
using CustomCodeFramework.Core.Domain.Entities;
using CustomCodeFramework.Messaging.Inbox;
using CustomCodeFramework.Messaging.Outbox;
using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using CustomCodeFramework.Postgres.EntityFramework.DbContexts;
using Dhole.Storage.Domain.Files.Entities;
using Dhole.Storage.Domain.Providers.Entities;
using Dhole.Storage.Persistence.Auditing;
using Dhole.Storage.Persistence.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dhole.Storage.Persistence.DbContexts;

public sealed class ServiceDbContext(DbContextOptions<ServiceDbContext> options,
    IConfiguration? realtimeConfiguration = null)
    : AppDbContextBase(options)
{
    private const string SourceService = "DholeStorageService";

    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Dhole.Storage.Domain.Files.Entities.File> Files =>
        Set<Dhole.Storage.Domain.Files.Entities.File>();
    public DbSet<FileVersion> FileVersions => Set<FileVersion>();
    public DbSet<FileReference> FileReferences => Set<FileReference>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AppendStorageFileInvalidations();
        AddDomainEventsToOutbox();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        AppendStorageFileInvalidations();
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

    private void AppendStorageFileInvalidations()
    {
        if (!StorageRealtimeInvalidation.IsEnabled(realtimeConfiguration))
            return;
        ChangeTracker.DetectChanges();
        var ids = new HashSet<Guid>();
        foreach (var entry in ChangeTracker.Entries()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            var fileId = entry.Entity switch
            {
                Dhole.Storage.Domain.Files.Entities.File file => file.Id,
                FileVersion version => version.FileId,
                FileReference reference => reference.FileId,
                _ => Guid.Empty
            };
            if (fileId != Guid.Empty)
                ids.Add(fileId);
        }
        foreach (var fileId in ids)
        {
            var eventId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var correlationId =
                (AuditExecutionContextAccessor.Current?.CorrelationId ?? eventId).ToString("D");
            OutboxMessages.Add(new OutboxMessage
            {
                EventId = eventId,
                EventType = StorageRealtimeInvalidation.Type,
                EventName = StorageRealtimeInvalidation.Type,
                SourceService = SourceService,
                PayloadJson = StorageRealtimeInvalidation.Serialize(
                    eventId, fileId, StorageRealtimeInvalidation.NextVersionHint(),
                    now, correlationId),
                CorrelationId = correlationId,
                Status = OutboxMessageStatus.Pending,
                RetryCount = 0,
                CreatedAtUtc = now.UtcDateTime
            });
        }
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

        var outboxMessages = new List<OutboxMessage>();

        foreach (var aggregateRoot in aggregateRoots)
        {
            foreach (var domainEvent in aggregateRoot.DomainEvents)
            {
                var eventType = DomainEventOutboxMapper.GetEventType(domainEvent);
                var eventName = DomainEventOutboxMapper.GetEventName(domainEvent);
                var payloadJson = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

                var correlationId =
                    AuditExecutionContextAccessor.Current?.CorrelationId ?? Guid.NewGuid();

                outboxMessages.Add(
                    new OutboxMessage
                    {
                        EventId = domainEvent.EventId,
                        EventType = eventType,
                        EventName = eventName,
                        SourceService = SourceService,
                        PayloadJson = payloadJson,
                        HeadersJson = null,
                        CorrelationId = correlationId.ToString(),
                        Status = OutboxMessageStatus.Pending,
                        RetryCount = 0,
                        ErrorMessage = null,
                        CreatedAtUtc = DateTime.UtcNow,
                    }
                );
            }

            aggregateRoot.ClearDomainEvents();
        }

        OutboxMessages.AddRange(outboxMessages);
    }
}

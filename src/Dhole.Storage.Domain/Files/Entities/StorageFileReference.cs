using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Storage.Domain.Files.Entities;

public sealed class StorageFileReference : SoftDeletableAggregateRoot<Guid>
{
    private StorageFileReference() { }

    private StorageFileReference(
        Guid id,
        Guid fileId,
        string sourceService,
        string entityType,
        Guid entityId,
        string? referenceType,
        Guid? createdBy
    )
        : base(id)
    {
        FileId = fileId;
        SourceService = sourceService;
        EntityType = entityType;
        EntityId = entityId;
        ReferenceType = referenceType;

        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public Guid FileId { get; private set; }
    public string SourceService { get; private set; } = default!;
    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public string? ReferenceType { get; private set; }

    internal static StorageFileReference Create(
        Guid fileId,
        string sourceService,
        string entityType,
        Guid entityId,
        string? referenceType,
        Guid? createdBy
    )
    {
        return new StorageFileReference(
            Guid.NewGuid(),
            fileId,
            sourceService.Trim(),
            entityType.Trim(),
            entityId,
            referenceType?.Trim(),
            createdBy
        );
    }
}

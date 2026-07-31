using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Storage.Domain.Files.Entities;

public sealed class FileReference : SoftDeletableAggregateRoot<Guid>
{
    private FileReference() { }

    private FileReference(
        Guid id,
        Guid fileId,
        string sourceService,
        string entityType,
        Guid entityId,
        Guid? createdBy
    )
        : base(id)
    {
        FileId = fileId;
        SourceService = sourceService;
        EntityType = entityType;
        EntityId = entityId;

        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public Guid FileId { get; private set; }
    public string SourceService { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }

    internal static FileReference Create(
        Guid fileId,
        string sourceService,
        string entityType,
        Guid entityId,
        Guid? createdBy
    )
    {
        return new FileReference(
            Guid.NewGuid(),
            fileId,
            sourceService,
            entityType,
            entityId,
            createdBy
        );
    }
}

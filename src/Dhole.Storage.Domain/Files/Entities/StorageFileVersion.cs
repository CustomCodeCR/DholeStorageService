using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Storage.Domain.Files.Entities;

public sealed class StorageFileVersion : SoftDeletableAggregateRoot<Guid>
{
    private StorageFileVersion() { }

    private StorageFileVersion(
        Guid id,
        Guid fileId,
        int versionNumber,
        string storedFileName,
        string storagePath,
        long sizeInBytes,
        string? checksum,
        Guid? createdBy
    )
        : base(id)
    {
        FileId = fileId;
        VersionNumber = versionNumber;
        StoredFileName = storedFileName;
        StoragePath = storagePath;
        SizeInBytes = sizeInBytes;
        Checksum = checksum;

        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public Guid FileId { get; private set; }
    public int VersionNumber { get; private set; }
    public string StoredFileName { get; private set; } = default!;
    public string StoragePath { get; private set; } = default!;
    public long SizeInBytes { get; private set; }
    public string? Checksum { get; private set; }

    internal static StorageFileVersion Create(
        Guid fileId,
        int versionNumber,
        string storedFileName,
        string storagePath,
        long sizeInBytes,
        string? checksum,
        Guid? createdBy
    )
    {
        return new StorageFileVersion(
            Guid.NewGuid(),
            fileId,
            versionNumber,
            storedFileName.Trim(),
            storagePath.Trim(),
            sizeInBytes,
            checksum,
            createdBy
        );
    }
}

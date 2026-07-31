using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Storage.Domain.Files.Entities;

public sealed class FileVersion : SoftDeletableAggregateRoot<Guid>
{
    private FileVersion() { }

    private FileVersion(
        Guid id,
        Guid fileId,
        string storedFileName,
        int versionNumber,
        string path,
        long sizeInBytes,
        string? checksum,
        Guid? createdBy
    )
        : base(id)
    {
        FileId = fileId;
        StoredFileName = storedFileName;
        VersionNumber = versionNumber;
        Path = path;
        SizeInBytes = sizeInBytes;
        Checksum = checksum;

        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public Guid FileId { get; private set; }
    public string StoredFileName { get; private set; } = string.Empty;
    public int VersionNumber { get; private set; }
    public string Path { get; private set; } = string.Empty;
    public long SizeInBytes { get; private set; }
    public string? Checksum { get; private set; }

    internal static FileVersion Create(
        Guid fileId,
        string storedFileName,
        int versionNumber,
        string path,
        long sizeInBytes,
        string? checksum,
        Guid? createdBy
    )
    {
        return new FileVersion(
            Guid.NewGuid(),
            fileId,
            storedFileName.Trim(),
            versionNumber,
            path.Trim(),
            sizeInBytes,
            checksum,
            createdBy
        );
    }
}

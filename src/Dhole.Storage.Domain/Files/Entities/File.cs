using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Storage.Domain.Files.Enums;
using Dhole.Storage.Domain.Files.Events;

namespace Dhole.Storage.Domain.Files.Entities;

public sealed class File : SoftDeletableAggregateRoot<Guid>
{
    private readonly List<FileVersion> _versions = [];
    private readonly List<FileReference> _references = [];

    private File() { }

    private File(
        Guid id,
        Guid providerId,
        string originalFileName,
        string storedFileName,
        string contentType,
        string? extension,
        long sizeInBytes,
        string path,
        string? checksum,
        string? metadataJson,
        Guid? createdBy
    )
        : base(id)
    {
        ProviderId = providerId;
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        ContentType = contentType;
        Extension = extension;
        SizeInBytes = sizeInBytes;
        Path = path;
        Checksum = checksum;
        MetadataJson = metadataJson;
        Status = FileStatus.Uploaded;
        CurrentVersionNumber = 1;

        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public Guid ProviderId { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string? Extension { get; private set; }
    public long SizeInBytes { get; private set; }
    public string Path { get; private set; } = string.Empty;
    public string? Checksum { get; private set; }
    public string? MetadataJson { get; private set; }
    public FileStatus Status { get; private set; }
    public int CurrentVersionNumber { get; private set; }

    public IReadOnlyCollection<FileVersion> Versions => _versions;
    public IReadOnlyCollection<FileReference> References => _references;

    public static File Upload(
        Guid providerId,
        string sourceService,
        string entityType,
        Guid entityId,
        string originalFileName,
        string storedFileName,
        string contentType,
        string? extension,
        long sizeInBytes,
        string path,
        string? checksum,
        string? metadataJson,
        Guid? uploadedBy
    )
    {
        var file = new File(
            Guid.NewGuid(),
            providerId,
            originalFileName,
            storedFileName,
            contentType,
            extension,
            sizeInBytes,
            path,
            checksum,
            metadataJson,
            uploadedBy
        );

        var version = FileVersion.Create(
            file.Id,
            file.StoredFileName,
            1,
            file.Path,
            file.SizeInBytes,
            file.Checksum,
            uploadedBy
        );

        var reference = FileReference.Create(
            file.Id,
            sourceService,
            entityType,
            entityId,
            uploadedBy
        );

        file._versions.Add(version);
        file._references.Add(reference);

        file.AddDomainEvent(
            new FileUploadedDomainEvent(
                file.Id,
                file.ProviderId,
                file.OriginalFileName,
                file.StoredFileName,
                file.Extension,
                uploadedBy
            )
        );

        return file;
    }

    public FileVersion AddVersion(
        string storedFileName,
        string path,
        long sizeInBytes,
        string? checksum,
        Guid? uploadedBy
    )
    {
        var nextVersionNumber = _versions.Count == 0 ? 1 : _versions.Max(x => x.VersionNumber) + 1;

        var version = FileVersion.Create(
            Id,
            storedFileName,
            nextVersionNumber,
            path,
            sizeInBytes,
            checksum,
            uploadedBy
        );

        _versions.Add(version);

        MarkAsUpdated(DateTime.UtcNow, uploadedBy?.ToString());

        AddDomainEvent(
            new FileVersionUploadedDomainEvent(
                Id,
                version.Id,
                version.VersionNumber,
                OriginalFileName,
                version.StoredFileName,
                version.Path,
                version.SizeInBytes,
                version.Checksum,
                uploadedBy
            )
        );

        return version;
    }

    public void SetCurrentVersion(int versionNumber, Guid? changedBy)
    {
        var version = _versions.FirstOrDefault(x => x.VersionNumber == versionNumber);

        if (version is null)
            throw new InvalidOperationException("La version del archivo no existe.");

        CurrentVersionNumber = version.VersionNumber;
        StoredFileName = version.StoredFileName;
        Path = version.Path;
        SizeInBytes = version.SizeInBytes;
        Checksum = version.Checksum;

        MarkAsUpdated(DateTime.UtcNow, changedBy?.ToString());

        AddDomainEvent(
            new FileCurrentVersionChangedDomainEvent(
                Id,
                version.Id,
                version.VersionNumber,
                changedBy
            )
        );
    }

    public void Delete(Guid? deletedBy)
    {
        Status = FileStatus.Deleted;

        MarkAsDeleted(DateTime.UtcNow, deletedBy?.ToString());

        var reference = _references.FirstOrDefault();

        AddDomainEvent(
            new FileDeletedDomainEvent(
                Id,
                ProviderId,
                OriginalFileName,
                StoredFileName,
                Extension,
                deletedBy
            )
        );
    }
}

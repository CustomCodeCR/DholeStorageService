using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Storage.Domain.Files.Events;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Domain.Files.Entities;

public sealed class StorageFile : SoftDeletableAggregateRoot<Guid>
{
    private readonly List<StorageFileVersion> _versions = [];
    private readonly List<StorageFileReference> _references = [];

    private StorageFile() { }

    private StorageFile(
        Guid id,
        Guid providerId,
        string originalFileName,
        string storedFileName,
        string contentType,
        string? extension,
        long sizeInBytes,
        string storagePath,
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
        StoragePath = storagePath;
        Checksum = checksum;
        MetadataJson = metadataJson;
        Status = StorageConstants.FileStatuses.Uploaded;
        CurrentVersionNumber = 1;

        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public Guid ProviderId { get; private set; }
    public string OriginalFileName { get; private set; } = default!;
    public string StoredFileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public string? Extension { get; private set; }
    public long SizeInBytes { get; private set; }
    public string StoragePath { get; private set; } = default!;
    public string? Checksum { get; private set; }
    public string Status { get; private set; } = default!;
    public string? MetadataJson { get; private set; }
    public int CurrentVersionNumber { get; private set; }

    public IReadOnlyCollection<StorageFileVersion> Versions => _versions;
    public IReadOnlyCollection<StorageFileReference> References => _references;

    public static StorageFile Upload(
        Guid providerId,
        string sourceService,
        string entityType,
        Guid entityId,
        string? referenceType,
        string originalFileName,
        string storedFileName,
        string contentType,
        string? extension,
        long sizeInBytes,
        string storagePath,
        string? checksum,
        string? metadataJson,
        Guid? uploadedBy = null
    )
    {
        var file = new StorageFile(
            Guid.NewGuid(),
            providerId,
            originalFileName.Trim(),
            storedFileName.Trim(),
            contentType.Trim(),
            extension?.Trim(),
            sizeInBytes,
            storagePath.Trim(),
            checksum,
            metadataJson,
            uploadedBy
        );

        var version = StorageFileVersion.Create(
            file.Id,
            1,
            file.StoredFileName,
            file.StoragePath,
            file.SizeInBytes,
            file.Checksum,
            uploadedBy
        );

        var reference = StorageFileReference.Create(
            file.Id,
            sourceService,
            entityType,
            entityId,
            referenceType,
            uploadedBy
        );

        file._versions.Add(version);
        file._references.Add(reference);

        file.AddDomainEvent(
            new StorageFileUploadedDomainEvent(
                file.Id,
                file.ProviderId,
                sourceService,
                entityType,
                entityId,
                file.OriginalFileName,
                file.StoredFileName,
                file.ContentType,
                file.SizeInBytes,
                file.StoragePath,
                file.Checksum,
                version.VersionNumber,
                uploadedBy
            )
        );

        return file;
    }

    public StorageFileVersion AddVersion(
        string storedFileName,
        string storagePath,
        long sizeInBytes,
        string? checksum,
        Guid? uploadedBy = null
    )
    {
        var nextVersionNumber = _versions.Count == 0 ? 1 : _versions.Max(x => x.VersionNumber) + 1;

        var version = StorageFileVersion.Create(
            Id,
            nextVersionNumber,
            storedFileName,
            storagePath,
            sizeInBytes,
            checksum,
            uploadedBy
        );

        _versions.Add(version);

        StoredFileName = storedFileName.Trim();
        StoragePath = storagePath.Trim();
        SizeInBytes = sizeInBytes;
        Checksum = checksum;
        CurrentVersionNumber = nextVersionNumber;

        MarkAsUpdated(DateTime.UtcNow, uploadedBy?.ToString());

        AddDomainEvent(
            new StorageFileVersionUploadedDomainEvent(
                Id,
                version.Id,
                version.VersionNumber,
                OriginalFileName,
                version.StoredFileName,
                version.StoragePath,
                version.SizeInBytes,
                version.Checksum,
                uploadedBy
            )
        );

        return version;
    }

    public void SetCurrentVersion(int versionNumber, Guid? changedBy = null)
    {
        var version = _versions.FirstOrDefault(x => x.VersionNumber == versionNumber);

        if (version is null)
        {
            throw new InvalidOperationException("File version does not exist.");
        }

        CurrentVersionNumber = version.VersionNumber;
        StoredFileName = version.StoredFileName;
        StoragePath = version.StoragePath;
        SizeInBytes = version.SizeInBytes;
        Checksum = version.Checksum;

        MarkAsUpdated(DateTime.UtcNow, changedBy?.ToString());

        AddDomainEvent(
            new StorageFileCurrentVersionChangedDomainEvent(
                Id,
                version.Id,
                version.VersionNumber,
                changedBy
            )
        );
    }

    public void Delete(Guid? deletedBy = null)
    {
        Status = StorageConstants.FileStatuses.Deleted;

        MarkAsDeleted(DateTime.UtcNow, deletedBy?.ToString());

        var reference = _references.FirstOrDefault();

        AddDomainEvent(
            new StorageFileDeletedDomainEvent(
                Id,
                OriginalFileName,
                reference?.SourceService ?? StorageConstants.ServiceName,
                reference?.EntityType ?? "Unknown",
                reference?.EntityId ?? Guid.Empty,
                deletedBy
            )
        );
    }
}

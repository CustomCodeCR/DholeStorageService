using Dhole.Storage.Domain.Files.Entities;
using Dhole.Storage.Domain.Files.Events;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.UnitTests.Domain.Files;

[TestClass]
public sealed class StorageFileTests
{
    [TestMethod]
    public void Upload_ShouldCreateStorageFile()
    {
        var providerId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        var file = StorageFile.Upload(
            providerId,
            "Dhole.ConfigService",
            "CatalogGroup",
            entityId,
            "attachment",
            "document.pdf",
            "document-stored.pdf",
            "application/pdf",
            ".pdf",
            1024,
            "files/document-stored.pdf",
            "checksum",
            "{}",
            null
        );

        Assert.AreNotEqual(Guid.Empty, file.Id);
        Assert.AreEqual(providerId, file.ProviderId);
        Assert.AreEqual("document.pdf", file.OriginalFileName);
        Assert.AreEqual("document-stored.pdf", file.StoredFileName);
        Assert.AreEqual("application/pdf", file.ContentType);
        Assert.AreEqual(".pdf", file.Extension);
        Assert.AreEqual(1024, file.SizeInBytes);
        Assert.AreEqual("files/document-stored.pdf", file.StoragePath);
        Assert.AreEqual(StorageConstants.FileStatuses.Uploaded, file.Status);
        Assert.AreEqual(1, file.CurrentVersionNumber);
        Assert.AreEqual(1, file.Versions.Count);
        Assert.AreEqual(1, file.References.Count);
        Assert.IsTrue(file.DomainEvents.Any(x => x is StorageFileUploadedDomainEvent));
    }

    [TestMethod]
    public void AddVersion_ShouldCreateNewVersion()
    {
        var file = CreateFile();

        var version = file.AddVersion(
            "document-v2.pdf",
            "files/document-v2.pdf",
            2048,
            "checksum-v2",
            null
        );

        Assert.AreEqual(2, version.VersionNumber);
        Assert.AreEqual("document-v2.pdf", file.StoredFileName);
        Assert.AreEqual("files/document-v2.pdf", file.StoragePath);
        Assert.AreEqual(2048, file.SizeInBytes);
        Assert.AreEqual("checksum-v2", file.Checksum);
        Assert.AreEqual(2, file.CurrentVersionNumber);
        Assert.IsTrue(file.DomainEvents.Any(x => x is StorageFileVersionUploadedDomainEvent));
    }

    [TestMethod]
    public void SetCurrentVersion_ShouldChangeCurrentVersion()
    {
        var file = CreateFile();

        file.AddVersion("document-v2.pdf", "files/document-v2.pdf", 2048, "checksum-v2", null);
        file.SetCurrentVersion(1);

        Assert.AreEqual(1, file.CurrentVersionNumber);
        Assert.AreEqual("document-stored.pdf", file.StoredFileName);
        Assert.AreEqual("files/document-stored.pdf", file.StoragePath);
        Assert.AreEqual(1024, file.SizeInBytes);
        Assert.AreEqual("checksum", file.Checksum);
        Assert.IsTrue(file.DomainEvents.Any(x => x is StorageFileCurrentVersionChangedDomainEvent));
    }

    [TestMethod]
    public void Delete_ShouldSetDeleted()
    {
        var file = CreateFile();

        file.Delete();

        Assert.IsTrue(file.IsDeleted);
        Assert.AreEqual(StorageConstants.FileStatuses.Deleted, file.Status);
        Assert.IsTrue(file.DomainEvents.Any(x => x is StorageFileDeletedDomainEvent));
    }

    private static StorageFile CreateFile()
    {
        return StorageFile.Upload(
            Guid.NewGuid(),
            "Dhole.ConfigService",
            "CatalogGroup",
            Guid.NewGuid(),
            "attachment",
            "document.pdf",
            "document-stored.pdf",
            "application/pdf",
            ".pdf",
            1024,
            "files/document-stored.pdf",
            "checksum",
            "{}",
            null
        );
    }
}

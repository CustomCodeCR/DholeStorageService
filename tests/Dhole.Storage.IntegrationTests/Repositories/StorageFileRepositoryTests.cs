using Dhole.Storage.Domain.Files.Entities;
using Dhole.Storage.Domain.Providers.Entities;
using Dhole.Storage.Domain.Shared;
using Dhole.Storage.Persistence.Repositories;

namespace Dhole.Storage.IntegrationTests.Repositories;

[TestClass]
public sealed class StorageFileRepositoryTests : IntegrationTestBase
{
    [TestMethod]
    public async Task AddAsync_And_GetWithDetailsAsync_ShouldPersistStorageFile()
    {
        var repository = new StorageFileRepository(DbContext);

        var provider = CreateProvider();
        var entityId = Guid.NewGuid();

        var file = StorageFile.Upload(
            provider.Id,
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

        DbContext.StorageProviders.Add(provider);
        await repository.AddAsync(file);
        await DbContext.SaveChangesAsync();

        var result = await repository.GetWithDetailsAsync(file.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(file.Id, result.Id);
        Assert.AreEqual("document.pdf", result.OriginalFileName);
        Assert.AreEqual(1, result.Versions.Count);
        Assert.AreEqual(1, result.References.Count);
    }

    [TestMethod]
    public async Task GetByEntityAsync_ShouldReturnFilesForEntity()
    {
        var repository = new StorageFileRepository(DbContext);

        var provider = CreateProvider();
        var entityId = Guid.NewGuid();

        var file = StorageFile.Upload(
            provider.Id,
            "Dhole.ConfigService",
            "CatalogGroup",
            entityId,
            "attachment",
            "catalog.pdf",
            "catalog-stored.pdf",
            "application/pdf",
            ".pdf",
            2048,
            "files/catalog-stored.pdf",
            null,
            null,
            null
        );

        DbContext.StorageProviders.Add(provider);
        await repository.AddAsync(file);
        await DbContext.SaveChangesAsync();

        var result = await repository.GetByEntityAsync("CatalogGroup", entityId);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(file.Id, result.Single().Id);
        Assert.AreEqual("catalog.pdf", result.Single().OriginalFileName);
    }

    [TestMethod]
    public async Task GetForSelectAsync_ShouldReturnUploadedFiles()
    {
        var repository = new StorageFileRepository(DbContext);

        var provider = CreateProvider();
        var entityId = Guid.NewGuid();

        var file = StorageFile.Upload(
            provider.Id,
            "Dhole.StorageService",
            "Shipment",
            entityId,
            "invoice",
            "invoice.pdf",
            "invoice-stored.pdf",
            "application/pdf",
            ".pdf",
            4096,
            "files/invoice-stored.pdf",
            null,
            null,
            null
        );

        DbContext.StorageProviders.Add(provider);
        await repository.AddAsync(file);
        await DbContext.SaveChangesAsync();

        var result = await repository.GetForSelectAsync(
            "Dhole.StorageService",
            "Shipment",
            entityId,
            "invoice"
        );

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(file.Id, result.Single().Id);
        Assert.AreEqual("invoice.pdf", result.Single().OriginalFileName);
    }

    private static StorageProvider CreateProvider()
    {
        return StorageProvider.Create(
            "azure-blob",
            "Azure Blob",
            StorageConstants.ProviderTypes.AzureBlob,
            "{}",
            true,
            null
        );
    }
}

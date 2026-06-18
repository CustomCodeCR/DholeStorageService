using Dhole.Storage.Domain.Providers.Entities;
using Dhole.Storage.Domain.Shared;
using Dhole.Storage.Persistence.Repositories;

namespace Dhole.Storage.IntegrationTests.Repositories;

[TestClass]
public sealed class StorageProviderRepositoryTests : IntegrationTestBase
{
    [TestMethod]
    public async Task AddAsync_And_GetByCodeAsync_ShouldPersistStorageProvider()
    {
        var repository = new StorageProviderRepository(DbContext);

        var provider = StorageProvider.Create(
            "azure-blob",
            "Azure Blob",
            StorageConstants.ProviderTypes.AzureBlob,
            "{}",
            true,
            null
        );

        await repository.AddAsync(provider);
        await DbContext.SaveChangesAsync();

        var result = await repository.GetByCodeAsync("azure-blob");

        Assert.IsNotNull(result);
        Assert.AreEqual(provider.Id, result.Id);
        Assert.AreEqual("azure-blob", result.Code);
        Assert.AreEqual("Azure Blob", result.Name);
    }

    [TestMethod]
    public async Task ExistsByCodeAsync_ShouldReturnTrue_WhenStorageProviderExists()
    {
        var repository = new StorageProviderRepository(DbContext);

        var provider = StorageProvider.Create(
            "local",
            "Local Storage",
            StorageConstants.ProviderTypes.Local,
            "{}",
            false,
            null
        );

        await repository.AddAsync(provider);
        await DbContext.SaveChangesAsync();

        var exists = await repository.ExistsByCodeAsync("local");

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task GetDefaultAsync_ShouldReturnDefaultActiveProvider()
    {
        var repository = new StorageProviderRepository(DbContext);

        var provider = StorageProvider.Create(
            "default-provider",
            "Default Provider",
            StorageConstants.ProviderTypes.AzureBlob,
            "{}",
            true,
            null
        );

        await repository.AddAsync(provider);
        await DbContext.SaveChangesAsync();

        var result = await repository.GetDefaultAsync();

        Assert.IsNotNull(result);
        Assert.AreEqual(provider.Id, result.Id);
        Assert.IsTrue(result.IsDefault);
        Assert.IsTrue(result.IsActive);
    }

    [TestMethod]
    public async Task GetForSelectAsync_ShouldReturnActiveStorageProviders()
    {
        var repository = new StorageProviderRepository(DbContext);

        var provider = StorageProvider.Create(
            "azure-select",
            "Azure Select",
            StorageConstants.ProviderTypes.AzureBlob,
            "{}",
            true,
            null
        );

        await repository.AddAsync(provider);
        await DbContext.SaveChangesAsync();

        var result = await repository.GetForSelectAsync(
            StorageConstants.ProviderTypes.AzureBlob,
            "azure"
        );

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(provider.Id, result.Single().Id);
        Assert.AreEqual("Azure Select", result.Single().Name);
    }
}

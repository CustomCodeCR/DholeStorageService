using Dhole.Storage.Domain.Providers.Entities;
using Dhole.Storage.Domain.Providers.Events;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.UnitTests.Domain.Providers;

[TestClass]
public sealed class StorageProviderTests
{
    [TestMethod]
    public void Create_ShouldCreateStorageProvider()
    {
        var provider = StorageProvider.Create(
            "azure-blob",
            "Azure Blob",
            StorageConstants.ProviderTypes.AzureBlob,
            "{}",
            true,
            null
        );

        Assert.AreNotEqual(Guid.Empty, provider.Id);
        Assert.AreEqual("azure-blob", provider.Code);
        Assert.AreEqual("Azure Blob", provider.Name);
        Assert.AreEqual(StorageConstants.ProviderTypes.AzureBlob, provider.ProviderType);
        Assert.IsTrue(provider.IsDefault);
        Assert.IsTrue(provider.IsActive);
        Assert.IsTrue(provider.DomainEvents.Any(x => x is StorageProviderCreatedDomainEvent));
    }

    [TestMethod]
    public void Update_ShouldUpdateStorageProvider()
    {
        var provider = StorageProvider.Create(
            "local",
            "Local",
            StorageConstants.ProviderTypes.Local,
            "{}",
            false,
            null
        );

        provider.Update("Local Updated", "{\"path\":\"/tmp\"}", true);

        Assert.AreEqual("Local Updated", provider.Name);
        Assert.AreEqual("{\"path\":\"/tmp\"}", provider.Configuration);
        Assert.IsTrue(provider.IsDefault);
        Assert.IsTrue(provider.DomainEvents.Any(x => x is StorageProviderUpdatedDomainEvent));
    }

    [TestMethod]
    public void Inactivate_ShouldSetInactive()
    {
        var provider = StorageProvider.Create(
            "local",
            "Local",
            StorageConstants.ProviderTypes.Local,
            "{}",
            false,
            null
        );

        provider.Inactivate();

        Assert.IsFalse(provider.IsActive);
        Assert.IsTrue(provider.DomainEvents.Any(x => x is StorageProviderInactivatedDomainEvent));
    }

    [TestMethod]
    public void Activate_ShouldSetActive()
    {
        var provider = StorageProvider.Create(
            "local",
            "Local",
            StorageConstants.ProviderTypes.Local,
            "{}",
            false,
            null
        );

        provider.Inactivate();
        provider.Activate();

        Assert.IsTrue(provider.IsActive);
        Assert.IsTrue(provider.DomainEvents.Any(x => x is StorageProviderActivatedDomainEvent));
    }
}

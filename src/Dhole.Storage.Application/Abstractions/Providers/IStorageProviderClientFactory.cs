namespace Dhole.Storage.Application.Abstractions.Providers;

public interface IStorageProviderClientFactory
{
    IStorageProviderClient GetClient(string providerType);
}

namespace Dhole.Storage.Contracts.Providers;

public sealed record StorageProviderSelectDto(
    Guid Id,
    string Code,
    string Name,
    string ProviderType
);

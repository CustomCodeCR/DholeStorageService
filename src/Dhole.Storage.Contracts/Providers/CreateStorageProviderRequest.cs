namespace Dhole.Storage.Contracts.Providers;

public sealed record CreateStorageProviderRequest(
    string Code,
    string Name,
    string ProviderType,
    bool IsDefault,
    string? Configuration
);

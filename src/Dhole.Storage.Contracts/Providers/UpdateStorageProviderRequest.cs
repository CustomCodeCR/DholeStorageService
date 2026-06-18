namespace Dhole.Storage.Contracts.Providers;

public sealed record UpdateStorageProviderRequest(
    string Name,
    bool IsDefault,
    string? Configuration
);

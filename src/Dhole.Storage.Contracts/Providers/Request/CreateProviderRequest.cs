namespace Dhole.Storage.Contracts.Providers.Request;

public sealed record CreateProviderRequest(
    string Name,
    string ProviderType,
    bool IsDefault,
    string? Configuration
);

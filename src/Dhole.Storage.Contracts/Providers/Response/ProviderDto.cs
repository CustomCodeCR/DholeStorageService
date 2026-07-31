namespace Dhole.Storage.Contracts.Providers.Response;

public sealed record ProviderDto(
    Guid Id,
    string Code,
    string Name,
    string ProviderType,
    bool IsDefault,
    bool IsActive,
    string? Configuration
);

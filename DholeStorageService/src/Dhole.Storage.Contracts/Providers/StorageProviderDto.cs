namespace Dhole.Storage.Contracts.Providers;

public sealed record StorageProviderDto(
    Guid Id,
    string Code,
    string Name,
    string ProviderType,
    bool IsDefault,
    bool IsActive,
    string? Configuration
);

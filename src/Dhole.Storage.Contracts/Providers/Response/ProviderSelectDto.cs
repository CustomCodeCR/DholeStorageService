namespace Dhole.Storage.Contracts.Providers.Response;

public sealed record ProviderSelectDto(Guid Id, string Code, string Name, string ProviderType);

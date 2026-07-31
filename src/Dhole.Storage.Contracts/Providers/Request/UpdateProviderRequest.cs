namespace Dhole.Storage.Contracts.Providers.Request;

public sealed record UpdateProviderRequest(string Name, bool IsDefault, string? Configuration);

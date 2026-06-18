namespace Dhole.Storage.Contracts.Files;

public sealed record GetFilesByEntityRequest(string EntityType, Guid EntityId);

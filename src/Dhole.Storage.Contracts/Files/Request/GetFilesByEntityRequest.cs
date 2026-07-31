namespace Dhole.Storage.Contracts.Files.Request;

public sealed record GetFIlesByEntityRequest(string EntityType, Guid EntityId);

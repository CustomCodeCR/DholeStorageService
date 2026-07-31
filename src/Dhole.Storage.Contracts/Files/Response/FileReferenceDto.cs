namespace Dhole.Storage.Contracts.Files.Response;

public sealed record FileReferenceDto(
    Guid Id,
    string SourceService,
    string EntityType,
    Guid EntityId
);

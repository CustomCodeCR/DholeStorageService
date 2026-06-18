namespace Dhole.Storage.Contracts.Files;

public sealed record StorageFileReferenceDto(
    Guid Id,
    string SourceService,
    string EntityType,
    Guid EntityId,
    string? ReferenceType
);

namespace Dhole.Storage.Contracts.Files;

public sealed record UploadFileRequest(
    Guid ProviderId,
    string SourceService,
    string EntityType,
    Guid EntityId,
    string? ReferenceType,
    string? MetadataJson
);

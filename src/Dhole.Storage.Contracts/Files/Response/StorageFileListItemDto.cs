namespace Dhole.Storage.Contracts.Files.Response;

public sealed record StorageFileListItemDto(
    Guid Id,
    Guid ProviderId,
    string ProviderName,
    string ProviderType,
    string OriginalFileName,
    string ContentType,
    string? Extension,
    long SizeInBytes,
    string? Checksum,
    string Status,
    int CurrentVersionNumber,
    DateTime CreatedAt,
    string? SourceService,
    string? EntityType,
    Guid? EntityId,
    int ReferenceCount,
    int VersionCount,
    string? MetadataJson
);

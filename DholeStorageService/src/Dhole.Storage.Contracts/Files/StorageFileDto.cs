namespace Dhole.Storage.Contracts.Files;

public sealed record StorageFileDto(
    Guid Id,
    Guid ProviderId,
    string OriginalFileName,
    string StoredFileName,
    string ContentType,
    string? Extension,
    long SizeInBytes,
    string StoragePath,
    string? Checksum,
    string Status,
    int CurrentVersionNumber,
    string? MetadataJson,
    DateTime CreatedAt,
    IReadOnlyCollection<StorageFileReferenceDto> References,
    IReadOnlyCollection<StorageFileVersionDto> Versions
);

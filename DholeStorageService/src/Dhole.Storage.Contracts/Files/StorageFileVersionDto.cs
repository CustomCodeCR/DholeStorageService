namespace Dhole.Storage.Contracts.Files;

public sealed record StorageFileVersionDto(
    Guid Id,
    int VersionNumber,
    string StoredFileName,
    string StoragePath,
    long SizeInBytes,
    string? Checksum,
    DateTime CreatedAt
);

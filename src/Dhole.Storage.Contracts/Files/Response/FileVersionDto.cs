namespace Dhole.Storage.Contracts.Files.Response;

public sealed record FileVersionDto(
    Guid Id,
    int VersionNumber,
    string StoredFileName,
    string Path,
    long SizeInBytes,
    string? Checksum,
    DateTime CreatedAt
);

namespace Dhole.Storage.Contracts.Files.Response;

public sealed record StoredFileResponse(
    Guid Id,
    string Reference,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    string? Checksum,
    int VersionNumber,
    Guid ProviderId,
    string ProviderType,
    string Path
);

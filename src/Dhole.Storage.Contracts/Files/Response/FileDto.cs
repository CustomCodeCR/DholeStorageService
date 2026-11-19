namespace Dhole.Storage.Contracts.Files.Response;

public sealed record FileDto(
    Guid Id,
    Guid ProviderId,
    string OriginalFileName,
    string StoredFileName,
    string ContentType,
    string? Extension,
    long SizeInBytes,
    string Path,
    string? Checksum,
    string Status,
    int CurrentVersionNumber,
    string? MetadatJson,
    DateTime CreatedAt,
    IReadOnlyCollection<FileReferenceDto> References,
    IReadOnlyCollection<FileVersionDto> Versions
)\n{\n    // Keep the legacy JSON field while exposing the correctly spelled metadataJson.\n    public string? MetadataJson => MetadatJson;\n}\n
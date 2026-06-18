namespace Dhole.Storage.Application.Abstractions.Files;

public sealed record StoredFileResult(
    string StoredFileName,
    string StoragePath,
    long SizeInBytes,
    string? Checksum
);

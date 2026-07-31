namespace Dhole.Storage.Contracts.Files.Response;

public sealed record FileSelectDto(
    Guid Id,
    string OriginalFileName,
    string StoredFileName,
    long SizeInBytes,
    string Status,
    int CurrentVersionNumber
);

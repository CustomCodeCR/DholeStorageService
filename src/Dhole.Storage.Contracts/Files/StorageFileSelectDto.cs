namespace Dhole.Storage.Contracts.Files;

public sealed record StorageFileSelectDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    string Status,
    int CurrentVersionNumber
);

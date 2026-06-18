namespace Dhole.Storage.Contracts.Files;

public sealed record DownloadFileResponse(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeInBytes
);

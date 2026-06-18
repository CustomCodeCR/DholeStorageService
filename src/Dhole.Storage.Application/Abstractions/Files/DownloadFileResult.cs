namespace Dhole.Storage.Application.Abstractions.Files;

public sealed record DownloadFileResult(
    Stream Stream,
    string FileName,
    string ContentType,
    long SizeInBytes
);

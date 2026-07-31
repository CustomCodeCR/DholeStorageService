namespace Dhole.Storage.Contracts.Files.Response;

public sealed record DownloadFileDto(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeInBytes
);

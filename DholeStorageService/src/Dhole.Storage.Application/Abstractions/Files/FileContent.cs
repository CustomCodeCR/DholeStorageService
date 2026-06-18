namespace Dhole.Storage.Application.Abstractions.Files;

public sealed record FileContent(
    Stream Stream,
    string FileName,
    string ContentType,
    long SizeInBytes
);

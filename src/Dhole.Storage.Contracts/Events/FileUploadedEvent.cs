namespace Dhole.Storage.Contracts.Events;

public sealed record FileUploadedEvent(
    Guid FileId,
    string SourceService,
    string EntityType,
    Guid EntityId,
    string FileName,
    string ContentType,
    long SizeInBytes,
    int VersionNumber
);

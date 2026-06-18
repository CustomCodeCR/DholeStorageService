namespace Dhole.Storage.Contracts.Events;

public sealed record FileVersionUploadedEvent(Guid FileId, Guid VersionId, int VersionNumber);

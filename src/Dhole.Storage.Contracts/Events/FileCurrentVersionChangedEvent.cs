namespace Dhole.Storage.Contracts.Events;

public sealed record FileCurrentVersionChangedEvent(Guid FileId, int VersionNumber);

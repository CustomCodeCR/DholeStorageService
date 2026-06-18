namespace Dhole.Storage.Application.Abstractions.Paths;

public interface IStoragePathResolver
{
    string ResolveFilePath(
        string sourceService,
        string entityType,
        Guid entityId,
        string storedFileName
    );

    string ResolveVersionPath(
        string sourceService,
        string entityType,
        Guid entityId,
        Guid fileId,
        int versionNumber,
        string storedFileName
    );
}

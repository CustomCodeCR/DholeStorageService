namespace Dhole.Storage.Application.Abstractions.Services;

public interface IPathResolver
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
        int versionNUmber,
        string storedFileName
    );
}

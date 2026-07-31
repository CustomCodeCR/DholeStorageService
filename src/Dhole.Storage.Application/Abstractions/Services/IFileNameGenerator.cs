namespace Dhole.Storage.Application.Abstractions.Services;

public interface IFileNameGenerator
{
    string GenerateStoredFileName(string originalFileName, string? extension = null);

    string GenerateVersionedStoredFileName(
        Guid fileId,
        int versionNumber,
        string originalFileName,
        string? extension = null
    );
}

namespace Dhole.Storage.Application.Abstractions.Naming;

public interface IStorageFileNameGenerator
{
    string GenerateStoredFileName(string originalFileName, string? extension = null);

    string GenerateVersionedStoredFileName(
        Guid fileId,
        int versionNumber,
        string originalFileName,
        string? extension = null
    );
}

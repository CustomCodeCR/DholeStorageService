using Dhole.Storage.Application.Abstractions.Paths;

namespace Dhole.Storage.Application.Paths;

public sealed class StoragePathResolver : IStoragePathResolver
{
    public string ResolveFilePath(
        string sourceService,
        string entityType,
        Guid entityId,
        string storedFileName
    )
    {
        return string.Join(
            '/',
            Sanitize(sourceService),
            Sanitize(entityType),
            entityId.ToString("N"),
            Sanitize(storedFileName)
        );
    }

    public string ResolveVersionPath(
        string sourceService,
        string entityType,
        Guid entityId,
        Guid fileId,
        int versionNumber,
        string storedFileName
    )
    {
        return string.Join(
            '/',
            Sanitize(sourceService),
            Sanitize(entityType),
            entityId.ToString("N"),
            fileId.ToString("N"),
            $"v{versionNumber}",
            Sanitize(storedFileName)
        );
    }

    private static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(character =>
                char.IsLetterOrDigit(character) || character is '-' or '_' or '.' ? character : '-'
            )
            .ToArray();

        var result = new string(chars).Trim('-');

        while (result.Contains("--", StringComparison.Ordinal))
        {
            result = result.Replace("--", "-", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(result) ? "unknown" : result;
    }
}

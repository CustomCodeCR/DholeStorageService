namespace Dhole.Storage.Infrastructure.Cache;

internal static class StorageCacheKeys
{
    public static string SignedDownloadUrl(Guid fileId)
    {
        return $"storage:files:{fileId:N}:signed-download-url";
    }

    public static string FilesByEntity(string entityType, Guid entityId)
    {
        return $"storage:entities:{Normalize(entityType)}:{entityId:N}:files";
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}

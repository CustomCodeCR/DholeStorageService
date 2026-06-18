namespace Dhole.Storage.Domain.Shared;

public static class StorageConstants
{
    public const string ServiceName = "Storage";

    public static class Scopes
    {
        public const string FilesCreate = "storage.files.create";
        public const string FilesView = "storage.files.view";
        public const string FilesDownload = "storage.files.download";
        public const string FilesDelete = "storage.files.delete";
        public const string FilesVersion = "storage.files.version";
    }

    public static class ProviderTypes
    {
        public const string Local = "Local";
        public const string S3 = "S3";
        public const string AzureBlob = "AzureBlob";
        public const string MinIO = "MinIO";
    }

    public static class FileStatuses
    {
        public const string Uploaded = "Uploaded";
        public const string Deleted = "Deleted";
        public const string Failed = "Failed";
    }

    public static class EventTypes
    {
        public const string FileUploaded = "storage.file.uploaded";
        public const string FileVersionUploaded = "storage.file.version_uploaded";
        public const string CurrentVersionChanged = "storage.file.current_version_changed";
        public const string FileDeleted = "storage.file.deleted";
    }
}

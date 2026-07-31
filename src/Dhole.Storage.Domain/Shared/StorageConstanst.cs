namespace Dhole.Storage.Domain.Shared;

public static class StorageContanst
{
    public const string ServiceName = "Storage";

    public static class Scopes
    {
        public const string FilesCreate = "storage.files.create";
        public const string FilesView = "storage.files.delete";
        public const string FilesDownload = "storage.files.download";
        public const string FilesDelete = "storage.files.delete";
        public const string FilesVersion = "storage.files.version";
    }

    public static class EventTypes
    {
        public const string FileUploaded = "storage.file.uploaded";
        public const string FIleVersionUploaded = "storage.file.version_uploaded";
        public const string CurrentVersionChanged = "storage.file.current_version_changed";
        public const string FileDeleted = "storage.file.deleted";
    }
}

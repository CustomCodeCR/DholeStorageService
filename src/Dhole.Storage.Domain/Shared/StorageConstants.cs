namespace Dhole.Storage.Domain.Shared;

public static class StorageConstants
{
    public const string ServiceName = "DholeStorageService";

    public static class Scopes
    {
        public const string FilesCreate = "storage.files.create";
        public const string FilesView = "storage.files.view";
        public const string FilesDownload = "storage.files.download";
        public const string FilesDelete = "storage.files.delete";
        public const string FilesVersion = "storage.files.version";
        public const string ProvidersView = "storage.providers.view";
        public const string ProvidersCreate = "storage.providers.create";
        public const string ProvidersUpdate = "storage.providers.update";
        public const string ProvidersSetActive = "storage.providers.set-active";

        // Alias temporal para código anterior.
        public const string ProvidersManage = ProvidersUpdate;
    }

    public static class EventTypes
    {
        public const string FileUploaded = "storage.file.uploaded";
        public const string FileVersionUploaded = "storage.file.version-uploaded";
        public const string CurrentVersionChanged = "storage.file.current-version-changed";
        public const string FileDeleted = "storage.file.deleted";
    }
}

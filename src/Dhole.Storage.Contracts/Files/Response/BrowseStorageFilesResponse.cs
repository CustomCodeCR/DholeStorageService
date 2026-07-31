namespace Dhole.Storage.Contracts.Files.Response;

public sealed record BrowseStorageFilesResponse(
    IReadOnlyCollection<StorageFileListItemDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

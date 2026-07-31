namespace Dhole.Storage.Contracts.Files.Response;

public sealed record StorageSummaryDto(
    int TotalFiles,
    long TotalSizeInBytes,
    int ImageFiles,
    int PdfFiles,
    int DownloadOnlyFiles,
    int ProviderCount,
    int ActiveProviderCount
);

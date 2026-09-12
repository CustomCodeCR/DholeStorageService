namespace Dhole.Storage.Contracts.Files.Response;

public sealed record MarketingStoredFileResponse(
    StoredFileResponse File,
    string Category,
    string? Format,
    int? Width,
    int? Height,
    double? DurationSeconds,
    IReadOnlyCollection<MarketingMediaVariantDto> Variants);

public sealed record MarketingMediaVariantDto(
    string Key,
    string FileName,
    string ContentType,
    string Path,
    long SizeInBytes);

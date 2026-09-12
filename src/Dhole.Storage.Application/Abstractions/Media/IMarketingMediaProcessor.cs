namespace Dhole.Storage.Application.Abstractions.Media;

public interface IMarketingMediaProcessor
{
    Task<MarketingMediaProcessingResult> ProcessAsync(
        MarketingMediaInput input,
        CancellationToken cancellationToken = default);
}

public sealed record MarketingMediaInput(
    string FileName,
    string ContentType,
    byte[] Content);

public sealed record MarketingMediaProcessingResult(
    string Category,
    string? Format,
    int? Width,
    int? Height,
    double? DurationSeconds,
    IReadOnlyCollection<GeneratedMediaAsset> Assets);

public sealed record GeneratedMediaAsset(
    string Key,
    string FileName,
    string ContentType,
    byte[] Content);

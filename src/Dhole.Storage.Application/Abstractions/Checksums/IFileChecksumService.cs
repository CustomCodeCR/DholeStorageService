namespace Dhole.Storage.Application.Abstractions.Checksums;

public interface IFileChecksumService
{
    Task<string> CalculateAsync(Stream stream, CancellationToken cancellationToken = default);
}

namespace Dhole.Storage.Application.Abstractions.Services;

public interface IChecksumService
{
    Task<string> CalculateAsync(Stream stream, CancellationToken cancellationToken = default);
}

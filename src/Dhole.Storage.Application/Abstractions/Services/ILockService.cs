namespace Dhole.Storage.Application.Abstractions.Services;

public interface ILockService
{
    Task<bool> AcquireAsync(
        string key,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    );

    Task ReleaseAsync(string key, CancellationToken cancellationToken = default);
}

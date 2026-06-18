namespace Dhole.Storage.Application.Abstractions.Locks;

public interface IStorageLockService
{
    Task<bool> AcquireAsync(
        string key,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    );

    Task ReleaseAsync(string key, CancellationToken cancellationToken = default);
}

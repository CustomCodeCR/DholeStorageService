using System.Collections.Concurrent;
using Dhole.Storage.Application.Abstractions.Locks;

namespace Dhole.Storage.Application.Locks;

public sealed class InMemoryStorageLockService : IStorageLockService
{
    private static readonly ConcurrentDictionary<string, LockEntry> Locks = new();

    public Task<bool> AcquireAsync(
        string key,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTime.UtcNow;
        var entry = new LockEntry(now.Add(expiration));

        var acquired =
            Locks.AddOrUpdate(
                key,
                _ => entry,
                (_, current) => current.ExpiresAtUtc <= now ? entry : current
            ) == entry;

        return Task.FromResult(acquired);
    }

    public Task ReleaseAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Locks.TryRemove(key, out _);

        return Task.CompletedTask;
    }

    private sealed record LockEntry(DateTime ExpiresAtUtc);
}

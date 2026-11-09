using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Dhole.Storage.Persistence.Messaging;

/// <summary>
/// F6.07: metadata-only file-change signal. Never publish file paths, names,
/// contents, checksums, signed URLs, provider credentials or reference targets.
/// REST access checks must run before each user-visible delivery.
/// </summary>
public static class StorageRealtimeInvalidation
{
    public const string ReleaseId = "rt-20261107-01";
    public const string Type = "storage.file.changed.v1";
    public const string Stream = "dhole.storage.realtime.v1";
    private static long _lastMicrosecond;

    public static bool IsEnabled(IConfiguration? config) =>
        config?.GetValue<bool>("Features:UniversalRealtimeEnabled") == true &&
        config.GetValue<bool>("Realtime:Outbox:Enabled") &&
        config.GetValue<bool>("Realtime:BatchApproved") &&
        string.Equals(config["Realtime:ReleaseId"], ReleaseId, StringComparison.Ordinal);

    // Event cursor hint only; owner REST snapshot remains authoritative
    // for multi-instance/out-of-order delivery.
    public static long NextVersionHint()
    {
        while (true)
        {
            var timestamp = (DateTimeOffset.UtcNow.Ticks - DateTimeOffset.UnixEpoch.Ticks) / 10;
            var last = Volatile.Read(ref _lastMicrosecond);
            var next = Math.Max(timestamp, last + 1);
            if (Interlocked.CompareExchange(ref _lastMicrosecond, next, last) == last) return next;
        }
    }

    public static string Serialize(Guid eventId, Guid fileId, long version,
        DateTimeOffset utc, string correlationId)
    {
        if (eventId == Guid.Empty || fileId == Guid.Empty ||
            version < 1 || utc.Offset != TimeSpan.Zero ||
            string.IsNullOrWhiteSpace(correlationId))
            throw new ArgumentException("Invalid Storage realtime file reference.");
        return JsonSerializer.Serialize(new
        {
            eventId,
            eventType = Type,
            schemaVersion = 1,
            aggregateType = "file",
            aggregateId = fileId.ToString("D"),
            aggregateVersion = version,
            occurredAtUtc = utc,
            producer = "DholeStorageService",
            correlationId,
            audienceRef = $"storage:file:{fileId:D}",
            payload = new { changedFields = new[] { "file" } }
        });
    }
}

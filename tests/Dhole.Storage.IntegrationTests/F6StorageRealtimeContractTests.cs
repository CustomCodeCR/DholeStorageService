using System.Text.Json;
using Dhole.Storage.Persistence.Messaging;
using Microsoft.Extensions.Configuration;

namespace Dhole.Storage.IntegrationTests;

[TestClass]
public sealed class F6StorageRealtimeContractTests
{
    [TestMethod]
    public void Storage_flags_are_fail_closed()
    {
        Assert.IsFalse(StorageRealtimeInvalidation.IsEnabled(null));
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Features:UniversalRealtimeEnabled"] = "true",
                ["Realtime:Outbox:Enabled"] = "true",
                ["Realtime:BatchApproved"] = "false",
                ["Realtime:ReleaseId"] = StorageRealtimeInvalidation.ReleaseId
            }).Build();
        Assert.IsFalse(StorageRealtimeInvalidation.IsEnabled(config));
        config["Realtime:BatchApproved"] = "true";
        Assert.IsTrue(StorageRealtimeInvalidation.IsEnabled(config));
        config["Realtime:ReleaseId"] = "other-release";
        Assert.IsFalse(StorageRealtimeInvalidation.IsEnabled(config));
    }

    [TestMethod]
    public void File_signal_is_metadata_only_and_well_scoped()
    {
        var id = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var json = StorageRealtimeInvalidation.Serialize(eventId, id,
            StorageRealtimeInvalidation.NextVersionHint(), DateTimeOffset.UtcNow, "f6-test");
        using var parsed = JsonDocument.Parse(json);
        var obj = parsed.RootElement;
        Assert.AreEqual(StorageRealtimeInvalidation.Type, obj.GetProperty("eventType").GetString());
        Assert.AreEqual(id, obj.GetProperty("aggregateId").GetGuid());
        Assert.AreEqual($"storage:file:{id:D}", obj.GetProperty("audienceRef").GetString());
        CollectionAssert.AreEquivalent(new[] { "changedFields" },
            obj.GetProperty("payload").EnumerateObject().Select(p => p.Name).ToArray());
        foreach (var banned in new[] { "path", "fileName", "contentType", "checksum", "credentials", "signedUrl", "storageKey" })
            Assert.IsFalse(json.Contains(banned, StringComparison.OrdinalIgnoreCase), banned);
    }

    [TestMethod]
    public void Invalid_file_reference_is_rejected()
    {
        var id = Guid.NewGuid();
        var a = StorageRealtimeInvalidation.NextVersionHint();
        Assert.IsTrue(StorageRealtimeInvalidation.NextVersionHint() > a);
        Assert.ThrowsExactly<ArgumentException>(() =>
            StorageRealtimeInvalidation.Serialize(id, Guid.Empty, 1,
                DateTimeOffset.UtcNow, "x"));
        Assert.ThrowsExactly<ArgumentException>(() =>
            StorageRealtimeInvalidation.Serialize(id, id, 0,
                DateTimeOffset.UtcNow, "x"));
    }
}

using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Messages;

namespace Dhole.Storage.Worker.Streams;

internal sealed class StorageFileUploadedStreamHandler(
    ILogger<StorageFileUploadedStreamHandler> logger
) : IRedisStreamMessageHandler
{
    public string MessageType => "storage.file.uploaded";

    public async Task HandleAsync(
        RedisStreamEnvelope envelope,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "Received Redis Stream event {MessageType} with id {MessageId}.",
            envelope.MessageType,
            envelope.MessageId
        );

        await Task.CompletedTask;
    }
}

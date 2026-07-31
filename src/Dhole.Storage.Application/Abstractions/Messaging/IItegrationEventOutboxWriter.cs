namespace Dhole.Storage.Application.Abstractions.Messaging;

public interface IIntegrationEventOutboxWriter
{
    Task WriteAsync(
        string eventType,
        string eventName,
        object payload,
        string? correlationId = null,
        CancellationToken cancellationToken = default
    );
}

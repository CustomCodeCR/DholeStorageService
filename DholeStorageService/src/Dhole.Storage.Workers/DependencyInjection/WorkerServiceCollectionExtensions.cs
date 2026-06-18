using CustomCodeFramework.Messaging.DependencyInjection;
using CustomCodeFramework.Messaging.Outbox.DependencyInjection;
using CustomCodeFramework.Redis.DependencyInjection;
using CustomCodeFramework.Redis.Streams.DependencyInjection;
using CustomCodeFramework.Workers.DependencyInjection;
using Dhole.Storage.Worker.Outbox;
using Dhole.Storage.Worker.Workers;

namespace Dhole.Storage.Worker.DependencyInjection;

public static class WorkerServiceCollectionExtensions
{
    public static IServiceCollection AddStorageWorker(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddCustomCodeRedis(configuration);
        services.AddCustomCodeRedisStreams(configuration);

        services.AddCustomCodeMessaging(configuration);
        services.AddCustomCodeMessagingOutbox(configuration);

        services.AddCustomCodeOutboxProcessor<StorageOutboxProcessor>();
        services.AddCustomCodeInboxProcessor<StorageInboxProcessor>();
        services.AddCustomCodeMessagingOutboxHostedServices();

        services.AddCustomCodeWorkers(configuration);

        services.AddCustomCodePeriodicWorker<StorageHealthCheckWorker>();

        return services;
    }
}

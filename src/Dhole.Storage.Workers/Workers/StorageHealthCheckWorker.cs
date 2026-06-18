using CustomCodeFramework.Workers.Abstractions;
using Dhole.Storage.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Storage.Worker.Workers;

internal sealed class StorageHealthCheckWorker(
    ServiceDbContext dbContext,
    ILogger<StorageHealthCheckWorker> logger
) : IBackgroundWorker
{
    public string Name => "storage.health-check";

    public async Task ExecuteAsync(
        IWorkerExecutionContext context,
        CancellationToken cancellationToken
    )
    {
        var filesCount = await dbContext.Files.CountAsync(x => !x.IsDeleted, cancellationToken);

        var deletedFilesCount = await dbContext.Files.CountAsync(
            x => x.IsDeleted,
            cancellationToken
        );

        logger.LogInformation(
            "Storage health check completed. Files: {FilesCount}, DeletedFiles: {DeletedFilesCount}.",
            filesCount,
            deletedFilesCount
        );
    }
}

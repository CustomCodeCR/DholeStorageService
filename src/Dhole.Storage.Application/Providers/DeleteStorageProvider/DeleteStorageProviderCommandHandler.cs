using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Application.Providers.DeleteStorageProvider;

public sealed class DeleteStorageProviderCommandHandler(
    IStorageProviderRepository providers,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteStorageProviderCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteStorageProviderCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await providers.GetByIdAsync(command.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(StorageErrors.ProviderNotFound);
        }

        entity.MarkAsDeleted(DateTime.UtcNow, command.DeletedBy?.ToString());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

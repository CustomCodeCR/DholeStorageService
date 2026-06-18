using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Application.Providers.ActivateStorageProvider;

public sealed class ActivateStorageProviderCommandHandler(
    IStorageProviderRepository providers,
    IUnitOfWork unitOfWork
) : ICommandHandler<ActivateStorageProviderCommand, Result>
{
    public async Task<Result> HandleAsync(
        ActivateStorageProviderCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await providers.GetByIdAsync(command.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(StorageErrors.ProviderNotFound);
        }

        entity.Activate(command.ActivatedBy);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

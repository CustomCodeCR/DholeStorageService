using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Application.Providers.InactivateStorageProvider;

public sealed class InactivateStorageProviderCommandHandler(
    IStorageProviderRepository providers,
    IUnitOfWork unitOfWork
) : ICommandHandler<InactivateStorageProviderCommand, Result>
{
    public async Task<Result> HandleAsync(
        InactivateStorageProviderCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await providers.GetByIdAsync(command.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(StorageErrors.ProviderNotFound);
        }

        entity.Inactivate(command.InactivatedBy);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

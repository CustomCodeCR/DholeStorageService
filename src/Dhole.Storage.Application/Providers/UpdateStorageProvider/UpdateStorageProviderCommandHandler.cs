using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Application.Providers.UpdateStorageProvider;

public sealed class UpdateStorageProviderCommandHandler(
    IStorageProviderRepository providers,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateStorageProviderCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateStorageProviderCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await providers.GetByIdAsync(command.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(StorageErrors.ProviderNotFound);
        }

        entity.Update(
            command.Request.Name,
            command.Request.Configuration,
            command.Request.IsDefault,
            command.UpdatedBy
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Domain.Providers.Entities;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Application.Providers.CreateStorageProvider;

public sealed class CreateStorageProviderCommandHandler(
    IStorageProviderRepository providers,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateStorageProviderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateStorageProviderCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var code = command.Request.Code.Trim();

        if (await providers.ExistsByCodeAsync(code, cancellationToken))
        {
            return Result.Failure<Guid>(StorageErrors.ProviderCodeAlreadyExists);
        }

        var entity = StorageProvider.Create(
            code,
            command.Request.Name,
            command.Request.ProviderType,
            command.Request.Configuration,
            command.Request.IsDefault,
            command.CreatedBy
        );

        await providers.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(entity.Id);
    }
}

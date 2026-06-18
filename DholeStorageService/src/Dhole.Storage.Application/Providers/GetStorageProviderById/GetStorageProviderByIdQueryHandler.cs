using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Contracts.Providers;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Application.Providers.GetStorageProviderById;

public sealed class GetStorageProviderByIdQueryHandler(IStorageProviderRepository providers)
    : IQueryHandler<GetStorageProviderByIdQuery, Result<StorageProviderDto>>
{
    public async Task<Result<StorageProviderDto>> HandleAsync(
        GetStorageProviderByIdQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await providers.GetByIdAsync(query.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure<StorageProviderDto>(StorageErrors.ProviderNotFound);
        }

        return Result.Success(
            new StorageProviderDto(
                entity.Id,
                entity.Code,
                entity.Name,
                entity.ProviderType,
                entity.IsDefault,
                entity.IsActive,
                entity.Configuration
            )
        );
    }
}

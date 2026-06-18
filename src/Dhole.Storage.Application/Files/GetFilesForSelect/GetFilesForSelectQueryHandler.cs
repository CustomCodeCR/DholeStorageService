using CustomCodeFramework.Cqrs.Queries;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Contracts.Files;

namespace Dhole.Storage.Application.Files.GetFilesForSelect;

public sealed class GetFilesForSelectQueryHandler(IStorageFileRepository files)
    : IQueryHandler<GetFilesForSelectQuery, IReadOnlyCollection<StorageFileSelectDto>>
{
    public Task<IReadOnlyCollection<StorageFileSelectDto>> HandleAsync(
        GetFilesForSelectQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return files.GetForSelectAsync(
            query.SourceService,
            query.EntityType,
            query.EntityId,
            query.Search,
            cancellationToken
        );
    }
}

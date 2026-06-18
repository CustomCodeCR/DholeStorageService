using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Contracts.Files;

namespace Dhole.Storage.Application.Files.GetFiles;

public sealed class GetFilesQueryHandler(IStorageFileRepository files)
    : IQueryHandler<GetFilesQuery, PagedResult<StorageFileDto>>
{
    public Task<PagedResult<StorageFileDto>> HandleAsync(
        GetFilesQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return files.GetPagedAsync(
            query.Page,
            query.SourceService,
            query.EntityType,
            query.EntityId,
            query.DocumentType,
            query.Search,
            query.Status,
            cancellationToken
        );
    }
}

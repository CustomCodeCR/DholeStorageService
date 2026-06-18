using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Storage.Contracts.Files;

namespace Dhole.Storage.Application.Files.GetFiles;

public sealed record GetFilesQuery(
    PageRequest Page,
    string? SourceService,
    string? EntityType,
    Guid? EntityId,
    string? DocumentType,
    string? Search,
    string? Status
) : IQuery<PagedResult<StorageFileDto>>;

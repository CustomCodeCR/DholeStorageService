using CustomCodeFramework.Cqrs.Queries;
using Dhole.Storage.Contracts.Files;

namespace Dhole.Storage.Application.Files.GetFilesForSelect;

public sealed record GetFilesForSelectQuery(
    string? SourceService,
    string? EntityType,
    Guid? EntityId,
    string? Search
) : IQuery<IReadOnlyCollection<StorageFileSelectDto>>;

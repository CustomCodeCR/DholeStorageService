using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Contracts.Files;
using Dhole.Storage.Domain.Files.Entities;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Application.Files.GetFileById;

public sealed class GetFileByIdQueryHandler(IStorageFileRepository files)
    : IQueryHandler<GetFileByIdQuery, Result<StorageFileDto>>
{
    public async Task<Result<StorageFileDto>> HandleAsync(
        GetFileByIdQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await files.GetWithDetailsAsync(query.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure<StorageFileDto>(StorageErrors.FileNotFound);
        }

        return Result.Success(ToDto(entity));
    }

    private static StorageFileDto ToDto(StorageFile file)
    {
        return new StorageFileDto(
            file.Id,
            file.ProviderId,
            file.OriginalFileName,
            file.StoredFileName,
            file.ContentType,
            file.Extension,
            file.SizeInBytes,
            file.StoragePath,
            file.Checksum,
            file.Status,
            file.CurrentVersionNumber,
            file.MetadataJson,
            file.CreatedAtUtc,
            file.References.Select(x => new StorageFileReferenceDto(
                    x.Id,
                    x.SourceService,
                    x.EntityType,
                    x.EntityId,
                    x.ReferenceType
                ))
                .ToList(),
            file.Versions.OrderByDescending(x => x.VersionNumber)
                .Select(x => new StorageFileVersionDto(
                    x.Id,
                    x.VersionNumber,
                    x.StoredFileName,
                    x.StoragePath,
                    x.SizeInBytes,
                    x.Checksum,
                    x.CreatedAtUtc
                ))
                .ToList()
        );
    }
}

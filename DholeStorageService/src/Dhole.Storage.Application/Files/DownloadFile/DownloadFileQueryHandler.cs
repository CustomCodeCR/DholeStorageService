using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Downloads;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Contracts.Files;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Application.Files.DownloadFile;

public sealed class DownloadFileQueryHandler(
    IStorageFileRepository files,
    IFileStorage fileStorage
) : IQueryHandler<DownloadFileQuery, Result<DownloadFileResponse>>
{
    public async Task<Result<DownloadFileResponse>> HandleAsync(
        DownloadFileQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await files.GetWithDetailsAsync(query.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure<DownloadFileResponse>(StorageErrors.FileNotFound);
        }

        if (entity.IsDeleted || entity.Status == StorageConstants.FileStatuses.Deleted)
        {
            return Result.Failure<DownloadFileResponse>(StorageErrors.FileDeleted);
        }

        var download = await fileStorage.DownloadAsync(
            new FileDownloadRequest { Path = entity.StoragePath },
            cancellationToken
        );

        if (download.IsFailure || download.File is null)
        {
            return Result.Failure<DownloadFileResponse>(StorageErrors.FileNotFound);
        }

        return Result.Success(
            new DownloadFileResponse(
                download.File.Content.Stream,
                entity.OriginalFileName,
                download.File.Content.ContentType,
                download.File.Content.Length ?? entity.SizeInBytes
            )
        );
    }
}

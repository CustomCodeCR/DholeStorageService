using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using CustomCodeFramework.Storage.Abstractions;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Application.Files.DeleteFile;

public sealed class DeleteFileCommandHandler(
    IStorageFileRepository files,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteFileCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteFileCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await files.GetWithDetailsAsync(command.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(StorageErrors.FileNotFound);
        }

        await fileStorage.DeleteAsync(entity.StoragePath, cancellationToken);

        entity.Delete(command.DeletedBy);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

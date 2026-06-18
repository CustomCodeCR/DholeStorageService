using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;
using CustomCodeFramework.Storage.Uploads;
using Dhole.Storage.Application.Abstractions.Checksums;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Domain.Files.Entities;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Application.Files.UploadFile;

public sealed class UploadFileCommandHandler(
    IFileStorage fileStorage,
    IStorageFileRepository files,
    IStorageProviderRepository providers,
    IFileChecksumService checksumService,
    IUnitOfWork unitOfWork
) : ICommandHandler<UploadFileCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        UploadFileCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var provider = await providers.GetByIdAsync(command.Request.ProviderId, cancellationToken);

        if (provider is null)
        {
            return Result.Failure<Guid>(StorageErrors.ProviderNotFound);
        }

        if (!provider.IsActive)
        {
            return Result.Failure<Guid>(StorageErrors.ProviderInactive);
        }

        var metadata = new Dictionary<string, string>
        {
            ["sourceService"] = command.Request.SourceService,
            ["entityType"] = command.Request.EntityType,
            ["entityId"] = command.Request.EntityId.ToString("D"),
        };

        if (!string.IsNullOrWhiteSpace(command.Request.ReferenceType))
        {
            metadata["referenceType"] = command.Request.ReferenceType;
        }

        var checksum = await checksumService.CalculateAsync(command.Content, cancellationToken);

        var upload = await fileStorage.UploadAsync(
            new FileUploadRequest
            {
                FileName = command.FileName,
                Content = command.Content,
                ContentType = command.ContentType,
                SizeInBytes = command.SizeInBytes,
                UploadedBy = command.CreatedBy?.ToString(),
                Options = new FileUploadOptions
                {
                    Folder = BuildFolder(
                        command.Request.SourceService,
                        command.Request.EntityType,
                        command.Request.EntityId
                    ),
                    Visibility = FileVisibility.Private,
                    Metadata = metadata,
                },
            },
            cancellationToken
        );

        if (upload.IsFailure || upload.File is null)
        {
            var error = upload.Errors.FirstOrDefault();

            return Result.Failure<Guid>(
                new Error(
                    error?.Code ?? "Storage.UploadFailed",
                    error?.Message ?? "File upload failed."
                )
            );
        }

        var stored = upload.File;

        var entity = StorageFile.Upload(
            provider.Id,
            command.Request.SourceService,
            command.Request.EntityType,
            command.Request.EntityId,
            command.Request.ReferenceType,
            command.FileName,
            stored.FileName,
            stored.ContentType,
            Path.GetExtension(command.FileName),
            stored.SizeInBytes,
            stored.Path,
            checksum,
            command.Request.MetadataJson,
            command.CreatedBy
        );

        await files.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(entity.Id);
    }

    private static string BuildFolder(string sourceService, string entityType, Guid entityId)
    {
        return string.Join(
            '/',
            Sanitize(sourceService),
            Sanitize(entityType),
            entityId.ToString("N")
        );
    }

    private static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(character =>
                char.IsLetterOrDigit(character) || character is '-' or '_' or '.' ? character : '-'
            )
            .ToArray();

        var result = new string(chars).Trim('-');

        while (result.Contains("--", StringComparison.Ordinal))
        {
            result = result.Replace("--", "-", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(result) ? "unknown" : result;
    }
}

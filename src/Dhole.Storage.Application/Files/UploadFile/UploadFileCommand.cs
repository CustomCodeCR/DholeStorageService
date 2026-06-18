using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using Dhole.Storage.Contracts.Files;

namespace Dhole.Storage.Application.Files.UploadFile;

public sealed record UploadFileCommand(
    UploadFileRequest Request,
    string FileName,
    string ContentType,
    long SizeInBytes,
    Stream Content,
    Guid? CreatedBy
) : ICommand<Result<Guid>>;

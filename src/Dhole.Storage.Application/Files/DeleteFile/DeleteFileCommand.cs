using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Storage.Application.Files.DeleteFile;

public sealed record DeleteFileCommand(Guid Id, Guid? DeletedBy) : ICommand<Result>;

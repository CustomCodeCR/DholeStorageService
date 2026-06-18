using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Storage.Contracts.Files;

namespace Dhole.Storage.Application.Files.GetFileById;

public sealed record GetFileByIdQuery(Guid Id) : IQuery<Result<StorageFileDto>>;

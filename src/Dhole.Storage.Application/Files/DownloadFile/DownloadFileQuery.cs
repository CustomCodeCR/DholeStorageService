using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Storage.Contracts.Files;

namespace Dhole.Storage.Application.Files.DownloadFile;

public sealed record DownloadFileQuery(Guid Id) : IQuery<Result<DownloadFileResponse>>;

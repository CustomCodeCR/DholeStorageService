using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Storage.Application.Providers.InactivateStorageProvider;

public sealed record InactivateStorageProviderCommand(Guid Id, Guid? InactivatedBy) : ICommand<Result>;

using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Storage.Application.Providers.DeleteStorageProvider;

public sealed record DeleteStorageProviderCommand(Guid Id, Guid? DeletedBy) : ICommand<Result>;

using CustomCodeFramework.Core.Abstractions;

namespace Dhole.Storage.Api.Security;

internal static class CurrentUserExtensions
{
    public static Guid? GetUserGuid(this ICurrentUser currentUser)
    {
        return Guid.TryParse(currentUser.UserId, out var userId) ? userId : null;
    }
}

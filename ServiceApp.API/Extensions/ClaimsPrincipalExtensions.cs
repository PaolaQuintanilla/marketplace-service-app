using System.Security.Claims;
using ServiceApp.Application.Common.Exceptions;

namespace ServiceApp.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Reads the authenticated user's id from the JWT, or throws if absent/invalid.</summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedException("The access token does not contain a valid user id.");
    }
}

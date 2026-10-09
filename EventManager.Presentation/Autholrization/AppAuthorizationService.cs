using System.Security.Claims;
using EventManager.Application.Authorization;
using EventManager.Domain.Exceptions;
using EventManager.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace EventManager.Presentation.Authorization;

public class AppAuthorizationService(IAuthorizationService authorization, HttpContextAccessor accessor) : IAppAuthorizationService
{
    public ICurrentUser CurrentUser => new CurrentUser(accessor);

    public async Task AuthorizeAsync<TResource>(TResource resource, string policyName, CancellationToken cancellation) where TResource: class
    {
        if (accessor.HttpContext?.User is not ClaimsPrincipal user)
            throw new ForbiddenException(resource, policyName, string.Empty, Domain.ValueObjects.UserRole.User);

        var result = await authorization.AuthorizeAsync(user, resource, policyName);

        if (!result.Succeeded)
        {
            var login = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var role = user.FindFirstValue(ClaimTypes.Role) is not string roleStr ? UserRole.User : Enum.Parse<UserRole>(roleStr);
            throw new ForbiddenException(resource, policyName, login, role);
        }
    }
}
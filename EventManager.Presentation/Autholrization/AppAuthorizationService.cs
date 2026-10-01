using System.Security.Claims;
using EventManager.Application.Authorization;
using EventManager.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace EventManager.Presentation.Authorization;

public class AppAuthorizationService(IAuthorizationService authorization, HttpContextAccessor accessor) : IAppAuthorizationService
{
    public async Task Authorize<TResource>(TResource resource, string policyName, CancellationToken cancellation)
    {
        if (accessor.HttpContext?.User is not ClaimsPrincipal user)
            throw new ForbiddenException(typeof(TResource).Name, string.Empty);

        var result = await authorization.AuthorizeAsync(user, resource, policyName);

        if (!result.Succeeded)
        {
            var login = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            throw new ForbiddenException(typeof(TResource).Name, login);
        }
    }
}
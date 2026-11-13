using System.Security.Claims;
using Authorization.AspNetCore.Private;
using Authorization.Domain;
using Authorization.Exceptions;
using Authorization.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Authorization.AspNetCore.Services;

public class AppAuthorizationService : IAppAuthorizationService
{
    public ICurrentUser CurrentUser { get; }

    readonly ClaimsPrincipal? _principal;

    readonly IAuthorizationService _authorization;

    public AppAuthorizationService(IAuthorizationService authorization, IHttpContextAccessor accessor)
    {
        _principal = accessor.HttpContext?.User;
        CurrentUser = new CurrentUser(_principal);
        _authorization = authorization;
    }

    public async Task AuthorizeAsync<TResource>(TResource resource, string policyName, CancellationToken cancellation) where TResource : class
    {
        if (!CurrentUser.IsAuthenticated || _principal is null)
            throw new ForbiddenException(resource, policyName, string.Empty, UserRole.User);

        var result = await _authorization.AuthorizeAsync(_principal, resource, policyName);

        if (!result.Succeeded)
            throw new ForbiddenException
            (
                resource, policyName,
                CurrentUser.Login,
                CurrentUser.Role
            );
    }
}
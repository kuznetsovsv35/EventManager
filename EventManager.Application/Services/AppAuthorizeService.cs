using EventManager.Application.Authorization;

namespace EventManager.Application.Services;

public abstract class AppAuthorizeService<TService>(IAppAuthorizationService appAuthorization)
{
    protected Task AuthorizeAsync(string policyName, CancellationToken cancellation)
        => appAuthorization.AuthorizeAsync(this, policyName, cancellation);

    protected ICurrentUser CurrentUser => appAuthorization.CurrentUser;
}
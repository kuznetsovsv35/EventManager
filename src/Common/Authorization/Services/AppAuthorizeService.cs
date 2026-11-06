using Authorization.Interfaces;

namespace Authorization.Services;

/// <summary>
/// Базовый класс для сервисов требующих поддержки авторизации.
/// </summary>
/// <typeparam name="TService"></typeparam>
/// <param name="appAuthorization"></param>
public abstract class AppAuthorizeService<TService>(IAppAuthorizationService appAuthorization)
{
    protected Task AuthorizeAsync(string policyName, CancellationToken cancellation)
        => appAuthorization.AuthorizeAsync(this, policyName, cancellation);

    protected ICurrentUser CurrentUser => appAuthorization.CurrentUser;
}
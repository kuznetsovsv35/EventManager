namespace EventManager.Application.Authorization;

/// <summary>
/// Интерфейс авторизации на ресурсе.
/// </summary>
public interface IAppAuthorizationService
{
    /// <summary>
    /// В случает отсутствия прав по политике бросает ForbiddenException.
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    /// <param name="resource"></param>
    /// <param name="policyName"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task AuthorizeAsync<TResource>(TResource resource, string policyName, CancellationToken cancellation) where TResource: class;
    /// <summary>
    /// Возвращает текущего пользователя.
    /// </summary>
    ICurrentUser CurrentUser { get; }    
}
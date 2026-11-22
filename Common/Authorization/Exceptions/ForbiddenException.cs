using Authorization.Domain;

namespace Authorization.Exceptions;

/// <summary>
/// Исключение - нет прав на выполнение операцией.
/// </summary>
public class ForbiddenException(object resource, string policyName, ICurrentUser? user, Exception? innerException) : UserOperationException("Доступ запрещен", user, innerException)
{
    public string ResourceName { get; } = resource?.GetType().Name ?? string.Empty;

    public string PolicyName { get; } = policyName;


    public ForbiddenException(object resource, string policyName, ICurrentUser? user)
        : this(resource, policyName, user, null) { }
}
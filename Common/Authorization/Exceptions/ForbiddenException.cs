using Authorization.Domain;

namespace Authorization.Exceptions;

/// <summary>
/// Исключение - нет прав на выполнение операцией.
/// </summary>
public class ForbiddenException : UserOperationException
{
    public string ResourceName { get; }

    public string PolicyName { get; }


    public ForbiddenException(object resource, string policyName, ICurrentUser? user)
        : this(resource, policyName, user, null) { }

    public ForbiddenException(object resource, string policyName, ICurrentUser? user, Exception? innerException)
        : base("Доступ запрещен", user, innerException)
    {
        ResourceName = resource?.GetType().Name ?? string.Empty;
        PolicyName = policyName;
    }
}
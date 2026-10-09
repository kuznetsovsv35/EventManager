using EventManager.Domain.ValueObjects;

namespace EventManager.Domain.Exceptions;

public class ForbiddenException : UserOperationException
{
    public string ResourceName { get; }

    public string PolicyName { get; }

    public ForbiddenException(object resource, string policyName, string login, UserRole role)
        : this(resource, policyName, login, role, null) { }

    public ForbiddenException(object resource, string policyName, string login, UserRole role, Exception? innerException)
        : base("Доступ запрещен", login, role, innerException) 
    { 
        ResourceName = resource?.GetType().Name ?? string.Empty;
        PolicyName = policyName; 
    }
}
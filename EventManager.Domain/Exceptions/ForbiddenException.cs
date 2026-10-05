using EventManager.Domain.ValueObjects;

namespace EventManager.Domain.Exceptions;

public class ForbiddenException<TResource> : UserOperationException
{
    public string ResourceName => typeof(TResource).Name;

    public string PolicyName { get; }

    public ForbiddenException(string policyName, string login, UserRole role)
        : this(policyName, login, role, null) { }

    public ForbiddenException(string policyName, string login, UserRole role, Exception? innerException)
        : base("Доступ запрещен", login, role, innerException) { PolicyName = policyName; }
}
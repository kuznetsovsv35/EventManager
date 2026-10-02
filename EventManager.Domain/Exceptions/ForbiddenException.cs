using EventManager.Domain.ValueObjects;

namespace EventManager.Domain.Exceptions;

public class ForbiddenException<TResource> : UserOperationException
{
    public string ResourceName => typeof(TResource).Name;

    public string PolicyName { get; }

    public ForbiddenException(string message, string policyName, string login, UserRole role)
        : this(message, policyName, login, role, null) { }

    public ForbiddenException(string message, string policyName, string login, UserRole role, Exception? innerException)
        : base(message, login, role, innerException) { PolicyName = policyName; }
}
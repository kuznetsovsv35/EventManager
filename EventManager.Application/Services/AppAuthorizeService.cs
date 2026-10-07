using EventManager.Application.DataTransferObjects;
using EventManager.Domain.Exceptions;
using EventManager.Domain.ValueObjects;

namespace EventManager.Application.Services;

public abstract class AppAuthorizeService<TService>
{
    protected static void CheckUserRole(string policyName, UserInfo userInfo, UserRole requiredRole)
    {
        if (userInfo.Role < requiredRole)
            throw new ForbiddenException<TService>(policyName, userInfo.Login, userInfo.Role);
        
    }    
}
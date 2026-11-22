using System.Security.Claims;
using Authorization.Domain;

namespace Authorization.AspNetCore.Private;

class CurrentUser : ICurrentUser
{
    public Guid Id {get; }
    public string Login { get; }
    public UserRole Role { get; }
    public bool IsAuthenticated => _principal?.Identity?.IsAuthenticated ?? false;
    
    readonly ClaimsPrincipal? _principal;
    
    internal CurrentUser(ClaimsPrincipal? principal)
    {
        _principal = principal;
        Id  = GetClaim(ClaimTypes.NameIdentifier) is string id ? Guid.Parse(id) : Guid.Empty;
        Login = GetClaim(ClaimTypes.Name) ?? string.Empty;
        Role = GetClaim(ClaimTypes.Role) is string role ? Enum.Parse<UserRole>(role) : UserRole.User;
    }

    public bool InRole(UserRole role) => Role == role;

    string? GetClaim(string type) => _principal?.FindFirstValue(type);
}
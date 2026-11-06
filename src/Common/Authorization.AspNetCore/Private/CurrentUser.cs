using System.Security.Claims;
using Authorization.Domain;
using Authorization.Interfaces;

namespace Authorization.AspNetCore.Private;

class CurrentUser(ClaimsPrincipal? principal) : ICurrentUser
{
    public Guid Id => GetClaim(ClaimTypes.NameIdentifier) is string id ? Guid.Parse(id) : Guid.Empty;

    public string Login => GetClaim(ClaimTypes.Name) ?? string.Empty;

    public UserRole Role => GetClaim(ClaimTypes.Role) is string role ? Enum.Parse<UserRole>(role) : UserRole.User;

    public bool IsAuthenticated => principal?.Identity?.IsAuthenticated ?? false;

    public bool InRole(UserRole role) => Role == role;

    string? GetClaim(string type) => principal?.FindFirstValue(type);
}
using System.Security.Claims;
using EventManager.Application.Authorization;
using EventManager.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace EventManager.Presentation.Authorization;

class CurrentUser(HttpContextAccessor accessor) : ICurrentUser
{
    public Guid Id => GetClaim(ClaimTypes.NameIdentifier) is string id ? Guid.Parse(id) : Guid.Empty;

    public string Login => GetClaim(ClaimTypes.Name) ?? string.Empty;

    public UserRole Role => GetClaim(ClaimTypes.Role) is string role ? Enum.Parse<UserRole>(role) : UserRole.User;

    public bool IsAuthenticated => GetUser()?.Identity?.IsAuthenticated ?? false;

    public bool InRole(UserRole role) => Role == role;

    ClaimsPrincipal GetUser() => accessor.HttpContext?.User 
        ?? throw new InvalidOperationException($"{nameof(accessor.HttpContext)}: has no valid context.");

    string? GetClaim(string type) => GetUser().FindFirstValue(type);
}
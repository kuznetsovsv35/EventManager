namespace EventManager.Infrastructure.Authorization;

using System.Security.Claims;
using EventManager.Application.DataTransferObjects;

public interface ITokenService
{
    string GenerateAccessToken(UserInfo userInfo, IEnumerable<Claim>? extraClaims);

    string GenerateRefreshToken();

    ClaimsPrincipal? ValidateAccessToken(string token);

    bool ValidateRefreshToken(string token, string storedToken);
}
using System.Security.Claims;
using EventManager.Application.DataTransferObjects;
using EventManager.Infrastructure.Authentication;

namespace EventManager.Infrastructure.Authorization;

public interface ITokenService
{
    AuthResult GenerateTokens(UserInfo userInfo, IEnumerable<Claim>? extraClaims);

    ClaimsPrincipal? ValidateAccessToken(string token);

    bool ValidateRefreshToken(string token, string storedToken);
}
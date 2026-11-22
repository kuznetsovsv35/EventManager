using System.Security.Claims;
using UsersService.Application.DTO;

namespace UsersService.Application;

public interface ITokenService
{
    AuthResult GenerateTokens(UserInfo userInfo, IEnumerable<Claim>? extraClaims);

    ClaimsPrincipal? ValidateAccessToken(string token);

    bool ValidateRefreshToken(string token, string storedToken);
}

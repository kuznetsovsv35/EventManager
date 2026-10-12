using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EventManager.Application.DataTransferObjects;
using EventManager.Infrastructure.Authentication;
using EventManager.Infrastructure.Authorization;
using EventManager.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventManager.Infrastructure.Services;

public class JwtTokenService(IOptions<JwtSettings> options) : ITokenService
{
    public JwtSettings Settings => options.Value;
   
    readonly JwtSecurityTokenHandler _jwtHandler = new();

    readonly TokenValidationParameters _tokenValidationParams = new TokenValidationParameters().Configure(options.Value);

    public AuthResult GenerateTokens(UserInfo userInfo, IEnumerable<Claim>? extraClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
            new(ClaimTypes.Name, userInfo.Login),
            new(ClaimTypes.Role, userInfo.Role.ToString()),
        };

        if (extraClaims is not null)
            claims.AddRange(extraClaims);

        var token = new JwtSecurityToken
        (
            issuer: Settings.Issuer,
            audience: Settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Settings.AccessTokenExpiredAfter),
            signingCredentials: new SigningCredentials(_tokenValidationParams.IssuerSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new(
            _jwtHandler.WriteToken(token),
            Guid.NewGuid().ToString("N"),
            Settings.AccessTokenExpiredAfter);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var principal = _jwtHandler.ValidateToken(token, _tokenValidationParams, out var _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public bool ValidateRefreshToken(string token, string storedToken) => token == storedToken;
}
